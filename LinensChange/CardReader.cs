using ACS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using LinensChange;

namespace LinensChange
{    
    internal enum CardAuthMode
    {
        Read,
        ReadWrite
    }

    public enum CardError
    {

    }

    public class CardReader
    {               
        public event Action CardDisconnected;
        public event Action<Card> CardConnected;
        // public event Action<CardError> CardError;

        private string _activeDevice;
        private List<string> _devices;

        int hContext, hCard, protocol;
        Card _activeCard;
        byte[] _uid;

        // Will be monitoring a single reader
        ModWinsCard.SCARD_READERSTATE[] readerStates;
        
        public CardReader()
        {
            _activeCard = null;
            hContext = 0;
            RefreshDeviceList();
            
            if (_devices.Count > 0)
                ActiveDevice = _devices[0];
            
            _tokenSource = new CancellationTokenSource();

            _token = _tokenSource.Token;
            _readerTask = Task.Run(new Action(ReaderTask), _token);
        }

        public Card ActiveCard
        {
            get
            {
                lock(_lockObj)
                {
                    return _activeCard;
                }
            }
        }

        public string ActiveDevice
        {
            get
            {
                return _activeDevice;
            }
            set
            {
                lock (_lockObj)
                {
                    _activeDevice = value;
                    readerStates = new ModWinsCard.SCARD_READERSTATE[1];
                    readerStates[0].RdrName = _activeDevice;
                    readerStates[0].RdrCurrState = ModWinsCard.SCARD_STATE_UNAWARE;
                }
            }
        }

        public IEnumerable<string> AvailableDevices
        {
            get
            {
                return _devices;
            }
        }

        public void RefreshDeviceList()
        {
            int retCode;

            lock (_lockObj)
            {
                if (hContext != 0)
                    ModWinsCard.SCardReleaseContext(hContext);

                _devices = new List<string>();                

                //Establish Context
                retCode = ModWinsCard.SCardEstablishContext(ModWinsCard.SCARD_SCOPE_USER, 0, 0, ref hContext);

                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    throw new CardReaderException("Establish context failed", retCode);

                // 2. List PC/SC card readers installed in the system
                int pcchReaders = 0;

                retCode = ModWinsCard.SCardListReaders(this.hContext, null, null, ref pcchReaders);

                if (retCode == ModWinsCard.SCARD_E_NO_READERS_AVAILABLE)
                    return;

                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    throw new CardReaderException("List readers failed", retCode);

                string ReaderList = "" + Convert.ToChar(0);
                int index;
                
                string rName = "";

                byte[] buffer = new byte[pcchReaders];

                // Fill reader list
                retCode = ModWinsCard.SCardListReaders(this.hContext, null, buffer, ref pcchReaders);

                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    throw new CardReaderException("List readers failed", retCode);

                index = 0;
                do
                {
                    rName = "";

                    while (buffer[index] != 0)
                        rName = rName + (char)buffer[index++];

                    if (!string.IsNullOrWhiteSpace(rName))
                    {
                        _devices.Add(rName);
                        rName = "";
                    }
                    index++;
                } while (buffer[index] != 0);
            }
        }        

        private byte[] _cmdGetUID = { 0xFF, 0x00, 0x00, 0x00, 0x04, 0xD4, 0x4A, 0x01, 0x00 };
        
        public byte[] GetUID()
        {
            if (_uid == null)
            {
                var result = Send(_cmdGetUID, 0, _cmdGetUID.Length, 0,
                                 res => IsResultValid(res, _cmdGetUID) && res.Length >= 10 && res[2] == 0x01,
                                 "Get UID failed");
                // TODO: Temp fix
                if (result[7] == 7)
                {
                    _uid = new byte[8];
                    _uid[0] = 0x88;
                    Array.Copy(result.Skip(8).Take(result[7]).ToArray(), 0, _uid, 1, 7);
                }
                else
                    _uid = result.Skip(8).Take(result[7]).ToArray();
            }
            return _uid;
        }

        byte[] _cmdReadBlock = { 0xFF, 0x00, 0x00, 0x00, 0x05, 
                                 0xD4, 0x40, 0x01, 0x30, 0x00 };

        public byte[] ReadBlock(int blockNum)
        {
            lock (_lockObj)
            {
                if (blockNum < 0)
                    throw new ArgumentOutOfRangeException("Block number should be positive");

                CheckAuthentication(blockNum, CardAuthMode.Read);

                // ClearBuffers();
                _cmdReadBlock[9] = (byte)blockNum;

                var result = Send(_cmdReadBlock, 0, _cmdReadBlock.Length, 16 + 5,
                    res => IsResultValid(res, _cmdReadBlock) && res[2] == 0, "Read failed");

                // Omit the last 2 bytes and return the result
                return result.Skip(3).Take(16).ToArray();
            }
        }


        byte[] _cmdUpdateBlock = { 0xFF, 0x00, 0x00, 0x00, 0x05, 
                                 0xD4, 0x40, 0x01, 0xA0, 0x00,
                                 0, 0, 0, 0, 0, 0, 0, 0,    // data low
                                 0, 0, 0, 0, 0, 0, 0, 0};   // data high

        public void WriteBlock(int blockNum, byte[] data)
        {
            lock (_lockObj)
            {
                if (data.Length != 16)
                    throw new ArgumentOutOfRangeException("Invalid block size");

                if (blockNum < 0)
                    throw new ArgumentOutOfRangeException("Block number should be positive");

                if (_activeCard == null)
                    throw new CardReaderException("Card not connected");

                CheckAuthentication(blockNum, CardAuthMode.ReadWrite);

                // ClearBuffers();
                _cmdUpdateBlock[9] = (byte)blockNum;

                Array.Copy(data, 0, _cmdUpdateBlock, 10, data.Length);

                Send(_cmdUpdateBlock, 0, _cmdUpdateBlock.Length, 5,
                    res => IsResultValid(res, _cmdUpdateBlock) && res[2] == 0, "Write failed");
            }
        }

        int _lastAuthSector = -1;
        CardAuthMode _lastAuthMode = CardAuthMode.Read;
        DateTime _lastAuthTime = DateTime.MinValue;

        const int MSecBetweenAuth = 200;
        const int MaxRepeats = 3;

        // FF0000000FD440016004AF9B20A54C5EDA7F3280

        byte[] _cmdAuthenticate = { 0xFF, 0x00, 0x00, 0x00, 0x0F, 0xD4, 0x40, 0x01, 
                                    0x00, 0x00,                            // Mode, Block num           
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00,    // Key
                                    0x00, 0x00, 0x00, 0x00                  // UID
                                  };

        byte[] _readKey = { 0, 0, 0, 0, 0, 0 };
        byte[] _readWriteKey = { 0, 0, 0, 0, 0, 0 };

        private void CheckAuthentication(int blockNum, CardAuthMode mode)
        {
            if (blockNum / 4 != _lastAuthSector || _lastAuthMode != mode || (DateTime.Now - _lastAuthTime).TotalMilliseconds > MSecBetweenAuth)
            {
                // Need to have UID before we can start authenticating
                if (_uid == null)
                    GetUID();
                
                _cmdAuthenticate[9] = (byte)blockNum;
                if (mode == CardAuthMode.Read)
                {
                    _cmdAuthenticate[8] = 0x60;
                    Array.Copy(_readKey, 0, _cmdAuthenticate, 10, 6);
                }
                else
                {
                    _cmdAuthenticate[8] = 0x61;
                    Array.Copy(_readWriteKey, 0, _cmdAuthenticate, 10, 6);
                }

                Array.Copy(_uid, _uid.Length - 4, _cmdAuthenticate, 16, 4);


                var result = Send(_cmdAuthenticate, 0, _cmdAuthenticate.Length, 5,
                    res => IsResultValid(res, _cmdAuthenticate) && res[2] == 0, "Authentication failed");

                _lastAuthSector = blockNum / 4;
                _lastAuthMode = mode;
                _lastAuthTime = DateTime.Now;
            }
            
        }

        private bool IsResultValid(byte[] result, byte[] src)
        {
            return result.Length >= 4 && result[result.Length - 2] == 0x90 && result[result.Length - 1] == 00
                && result[0] == src[5] + 1 && result[1] == src[6] + 1;
        }


        const int BufferSize = 263;

        byte[] _sendBuffer = new byte[BufferSize];
        byte[] _recvBuffer = new byte[BufferSize];
        public int _sendBufferLen, _recvBufferLen;

        private byte[] Send(byte[] buffer, int index, int count, int expectedResultLen, 
                            Func<byte[], bool> checkResult, string errorMessage)
        {
            int repeatCount = 0;
            while (true)
            {
                try
                {
                    var result = SendAPDU(buffer, index, count, expectedResultLen);
                    if (!checkResult(result))
                        throw new CardReaderException(errorMessage);
                    return result;
                }
                catch (CardReaderException ex)
                {
                    if (repeatCount++ < MaxRepeats)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    throw ex;
                }                
            }
        }

        private byte[] SendAPDU(byte[] buffer, int index, int count, int expectedResultLen)
        {
            ModWinsCard.SCARD_IO_REQUEST pioSendRequest;
            pioSendRequest.dwProtocol = 0;
            pioSendRequest.cbPciLength = 8;
            _recvBufferLen = expectedResultLen;

            int retCode = ModWinsCard.SCardTransmit(hCard, ref pioSendRequest, ref buffer[index], count, ref pioSendRequest, ref _recvBuffer[0], ref _recvBufferLen);

            if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                throw new CardReaderException("Transmit failed", retCode);

            // Probably we did not know result the result length in advance            
            if (expectedResultLen == 0)
            {
                expectedResultLen = _recvBufferLen;
                retCode = ModWinsCard.SCardTransmit(hCard, ref pioSendRequest, ref buffer[index], count, ref pioSendRequest, ref _recvBuffer[0], ref _recvBufferLen);

                if (retCode != ModWinsCard.SCARD_S_SUCCESS)
                    throw new CardReaderException("Transmit failed", retCode);
            }

            if (expectedResultLen != _recvBufferLen)
                throw new CardReaderException("Unexpected result length", _recvBufferLen);

            byte[] result = new byte[_recvBufferLen];
            Array.Copy(_recvBuffer, result, _recvBufferLen);
            return result;
        }

        CancellationToken _token;
        CancellationTokenSource _tokenSource;
        Task _readerTask;
        object _lockObj = new object();

        private void ReaderTask()
        {
            try
            {
                while (true)
                {
                    _token.ThrowIfCancellationRequested();

                    lock (_lockObj)
                    {
                        if (!string.IsNullOrWhiteSpace(_activeDevice))
                        {
                            // Monitor changes
                            int retCode = ModWinsCard.SCardGetStatusChange(hContext, 200, ref readerStates[0], 1);
                            if (retCode == ModWinsCard.SCARD_S_SUCCESS && ((readerStates[0].RdrEventState & ModWinsCard.SCARD_STATE_CHANGED) != 0))
                            {
                                if (_activeCard == null && (readerStates[0].RdrEventState & ModWinsCard.SCARD_STATE_PRESENT) != 0)
                                {
                                    // Card connected, try to connect
                                    retCode = ModWinsCard.SCardConnect(hContext, _activeDevice, ModWinsCard.SCARD_SHARE_SHARED,
                                                          ModWinsCard.SCARD_PROTOCOL_T0 | ModWinsCard.SCARD_PROTOCOL_T1, ref hCard, ref protocol);

                                    if (retCode == ModWinsCard.SCARD_S_SUCCESS)
                                    {
                                        byte[] atr = new byte[readerStates[0].ATRLength];
                                        Array.Copy(readerStates[0].ATRValue, atr, atr.Length);

                                        try
                                        {
                                            _activeCard = new Card { UId = GetUID() };
                                            if (CardConnected != null)
                                                CardConnected(_activeCard);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                                else
                                    if (((readerStates[0].RdrEventState & ModWinsCard.SCARD_STATE_EMPTY) != 0))
                                    {
                                        retCode = ModWinsCard.SCardDisconnect(hCard, ModWinsCard.SCARD_LEAVE_CARD);
                                        if (_activeCard != null)
                                        {
                                            _activeCard = null;
                                            
                                        }
                                        _lastAuthSector = -1;
                                        _uid = null;
                                        if (CardDisconnected != null)
                                            CardDisconnected();
                                    }
                            }
                            readerStates[0].RdrCurrState = readerStates[0].RdrEventState & ~ModWinsCard.SCARD_STATE_CHANGED;
                            // readerStates[0].RdrEventState = 0;
                        }
                    }

                    Task.Delay(200).Wait();
                }
            }
            catch (OperationCanceledException) { }
        }

        private bool disposed = false;
        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                _tokenSource.Cancel();
                _readerTask.Wait(1000);
                if (_activeCard != null)
                    ModWinsCard.SCardDisconnect(hCard, ModWinsCard.SCARD_LEAVE_CARD);
                if (hContext != 0)
                    ModWinsCard.SCardReleaseContext(hContext);
                hCard = 0;
                hContext = 0;
                disposed = true;
            }
        }

        ~CardReader()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

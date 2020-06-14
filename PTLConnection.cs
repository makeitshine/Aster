using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTLStation
{
    class PTLMessage
    {
        public int ret { get; set; }
        public int nGwId { get; set; }
        public int nNode { get; set; }
        public short TagCommand { get; set; }
        public short nMsgType { get; set; }
        public int[] status { get; set; }
        public byte[] ccbdata { get; set; }
        public int ccblen { get; set; }
    }

    public class PTLConnection
    {
        private static int[] Gateway_N;
        /// <summary>
        /// Occurs when at least one of gateways connected or disconncted. The ConnStateArgs contains the statuses of all gateways.
        /// </summary>
        public event EventHandler<ConnStateArgs> connectionStatesChanged;
        /// <summary>
        /// Confirm button pressed when message displayed.
        /// ConfirmButtonArgs contains the Gateway, Node from which the message was received,
        /// the direction shows which buttons (up/down) were on when the mesage was confirmed and the message itself.
        /// </summary>
        public event EventHandler<ConfirmButtonArgs> ConfirmOccured;
        /// <summary>
        /// Confirm button pressed and the message has been modified by up/down buttons. 
        /// ConfirmButtonArgs contains the Gateway, Node from which the message was received,
        /// the direction shows which buttons (up/down) were on when the mesage was confirmed and the message itself.
        /// </summary>
        public event EventHandler<ConfirmButtonArgs> ShortageOccured;
        /// <summary>
        /// The stock mode confirmation.
        /// ConfirmButtonArgs contains the Gateway, Node from which the message was received,
        /// the direction shows which buttons (up/down) were on when the mesage was confirmed and the message itself.
        /// </summary>
        public event EventHandler<ConfirmButtonArgs> StockConfirmationOccured;
        /// <summary>
        /// Confirm button pressed with no message displayed. PressedArgs contains the gateway and node, where the button was pressed.
        /// </summary>
        public event EventHandler<PressedArgs> ConfirmPressed;
        /// <summary>
        /// Up button pressed with no message displayed. PressedArgs contains the gateway and node, where the button was pressed.
        /// </summary>
        public event EventHandler<PressedArgs> UpPressed;
        /// <summary>
        /// Down button pressed with no message displayed. PressedArgs contains the gateway and node, where the button was pressed.
        /// </summary>
        public event EventHandler<PressedArgs> DownPressed;

        BackgroundWorker bgwork;
        int[] curr_status;

        private bool connected;

        /// <summary>
        /// Initialize connection with PTL systems.
        /// </summary>
        /// <param name="gateway_nums">The array containing ids of gateways</param>
        public PTLConnection(int[] gateway_nums)
        {
            connected = false;
            Gateway_N = gateway_nums;
            curr_status = new int[Gateway_N.Length];
            for (int i = 0; i < Gateway_N.Length; i++)
            {
                curr_status[i] = -1;
            }
        }

        /// <summary>
        /// Establish connection with specified gateways. Returns -1 if failed, 1 if ok.
        /// </summary>
        /// <returns></returns>
        public int Connect()
        {
            if (!connected)
            {
                int retu = -1;
                
                try
                {
                    retu = CapsAPI.AB_API_Open();
                }
                catch { }

                if (retu < 0)
                {
                    return -1;
                }

                for (int i = 0; i < Gateway_N.Length; i++)
                {
                    CapsAPI.AB_GW_Open(Gateway_N[i]);
                }

                bgwork = new BackgroundWorker();
                bgwork.WorkerReportsProgress = true;
                bgwork.WorkerSupportsCancellation = true;
                bgwork.DoWork += (se, ev) =>
                {
                    while (true)
                    {
                        if (bgwork.CancellationPending)
                        {
                            break;
                        }

                        int[] status = new int[Gateway_N.Length];
                        for (int i = 0; i < Gateway_N.Length; i++)
                        {
                            status[i] = CapsAPI.AB_GW_Status(Gateway_N[i]);
                        }

                        byte[] ccbdata = new byte[255];
                        int ccblen = 0;

                        int ret = 0;
                        int nGwId, nNode;
                        short TagCommand, nMsg_type;
                        nGwId = 0;
                        nNode = 0;
                        TagCommand = -1;
                        nMsg_type = -1;
                        if (status[0] == 7)
                        {
                            ret = CapsAPI.AB_Tag_RcvMsg(ref nGwId, ref nNode, ref TagCommand, ref nMsg_type, ref ccbdata[0], ref ccblen);
                        }

                        bgwork.ReportProgress(0, new PTLMessage()
                        {
                            ret = ret,
                            nGwId = nGwId,
                            nMsgType = nMsg_type,
                            nNode = nNode,
                            TagCommand = TagCommand,
                            status = status,
                            ccbdata = ccbdata,
                            ccblen = ccblen
                        });
                        System.Threading.Thread.Sleep(10);
                    }
                };

                bgwork.ProgressChanged += (se, ev) =>
                {
                    int ret = (ev.UserState as PTLMessage).ret;
                    if (ret > 0)
                    {
                        switch ((ev.UserState as PTLMessage).TagCommand)
                        {
                            case 6:
                                {
                                    string m_RcvTagData = Bin2Str((ev.UserState as PTLMessage).ccbdata, 0, (ev.UserState as PTLMessage).ccblen);
                                    Direction dir = Direction.None;
                                    if (m_RcvTagData[0] != '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.None;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.Up;
                                    else if (m_RcvTagData[0] != '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Down;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Both;
                                    ConfirmOccured?.Invoke(this, new ConfirmButtonArgs()
                                    {
                                        Gateway = (ev.UserState as PTLMessage).nGwId,
                                        Node = -(ev.UserState as PTLMessage).nNode,
                                        Message = m_RcvTagData.Substring(3, 5),
                                        direction = dir
                                    });
                                    break;
                                }
                            case 7:
                                {
                                    string m_RcvTagData = Bin2Str((ev.UserState as PTLMessage).ccbdata, 0, (ev.UserState as PTLMessage).ccblen);
                                    Direction dir = Direction.None;
                                    if (m_RcvTagData[0] != '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.None;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.Up;
                                    else if (m_RcvTagData[0] != '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Down;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Both;
                                    ShortageOccured?.Invoke(this, new ConfirmButtonArgs()
                                    {
                                        Gateway = (ev.UserState as PTLMessage).nGwId,
                                        Node = -(ev.UserState as PTLMessage).nNode,
                                        Message = m_RcvTagData.Substring(3, 5),
                                        direction = dir
                                    });
                                    break;
                                }
                            case 15:
                                {
                                    string m_RcvTagData = Bin2Str((ev.UserState as PTLMessage).ccbdata, 0, (ev.UserState as PTLMessage).ccblen);
                                    Direction dir = Direction.None;
                                    if (m_RcvTagData[0] != '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.None;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] != '8')
                                        dir = Direction.Up;
                                    else if (m_RcvTagData[0] != '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Down;
                                    else if (m_RcvTagData[0] == '8' && m_RcvTagData[1] == '8')
                                        dir = Direction.Both;
                                    StockConfirmationOccured?.Invoke(this, new ConfirmButtonArgs()
                                    {
                                        Gateway = (ev.UserState as PTLMessage).nGwId,
                                        Node = -(ev.UserState as PTLMessage).nNode,
                                        Message = m_RcvTagData.Substring(3, 5),
                                        direction = dir
                                    });
                                    break;
                                }
                            case 100:
                                if ((ev.UserState as PTLMessage).nMsgType == 0)
                                {
                                    byte[] ccbdata = (ev.UserState as PTLMessage).ccbdata;
                                    int ccblen = (ev.UserState as PTLMessage).ccblen;
                                    int nRcvbun = CapsAPI.AB_GW_RcvButton(ref ccbdata[0], ref ccblen);
                                    switch (nRcvbun)
                                    {
                                        case 1:
                                            ConfirmPressed?.Invoke(this, new PressedArgs()
                                            {
                                                Gateway = (ev.UserState as PTLMessage).nGwId,
                                                Node = -(ev.UserState as PTLMessage).nNode
                                            });
                                            break;
                                        case 2:
                                            UpPressed?.Invoke(this, new PressedArgs()
                                            {
                                                Gateway = (ev.UserState as PTLMessage).nGwId,
                                                Node = -(ev.UserState as PTLMessage).nNode
                                            });
                                            break;
                                        case 3:
                                            DownPressed?.Invoke(this, new PressedArgs()
                                            {
                                                Gateway = (ev.UserState as PTLMessage).nGwId,
                                                Node = -(ev.UserState as PTLMessage).nNode
                                            });
                                            break;
                                    }
                                }
                                break;
                        }
                    }

                    int[] status = (ev.UserState as PTLMessage).status;
                    bool[] sta = new bool[status.Length];

                    bool changed = false;
                    for (int i = 0; i < status.Length; i++)
                    {
                        if ((curr_status[i] != 7 && status[i] == 7) || (curr_status[i] == 7 && status[i] != 7))
                        {
                            changed = true;
                        }
                        sta[i] = (status[i] == 7);
                        curr_status[i] = status[i];
                    }
                    if (changed)
                        connectionStatesChanged?.Invoke(this, new ConnStateArgs() { status = sta });
                };
                bgwork.RunWorkerAsync();
                connected = true;
                return 1;
            }
            else
                return -1;
        }

        public void Disconnect()
        {
            /*if (connected)
            {*/
                if (bgwork != null)
                    bgwork.CancelAsync();

                for (int i = 0; i < Gateway_N.Length; i++)
                {
                    CapsAPI.AB_GW_Close(Gateway_N[i]);
                    curr_status[i] = -1;
                }
                CapsAPI.AB_API_Close();
                connected = false;
            //}
        }

        /// <summary>
        /// Displays the number on the specified Node.
        /// </summary>
        /// <param name="gateway"> Gateway id</param>
        /// <param name="node"> Node id. (252 is for all nodes)</param>
        /// <param name="quantity"> The number to display </param>
        /// <param name="direction"> The direction to which PTL should point. Lights up the up/down arrows</param>
        /// <param name="Dot"> each bit for every dot. 0: off, 1: on </param>
        /// <param name="Interval"> 0: normal display;  -1: blinking with default frequency; >0: set blinking frequency (by msec);  -2: turn off digit;  -3: turn off digits and lamp</param>
        /*   public bool DisplayQuantity(int gateway, int node, int quantity, Direction direction, byte Dot, int Interval)
           {
               if (CapsAPI.AB_GW_Status(gateway) != 7)
               {
                   return false;
               }
               CapsAPI.AB_LB_SetMode(gateway, -node, 0);
               byte x = 1*1 + 2*0 + 4*1 + 8*1 + 16*1 + 32*1 + 64*0;
               int res = CapsAPI.AB_TAG_mode(gateway, -node, 0, x);

               CapsAPI.AB_LB5A_DspNum(gateway, -node, quantity, (byte)direction, Dot, Interval);
               return true;
           }

           /// <summary>
           /// Displays the number on the specified Node.
           /// </summary>
           /// <param name="gateway"> Gateway id</param>
           /// <param name="node"> Node id. (252 is for all nodes)</param>
           /// <param name="disp_string"> The string to display </param>
           /// <param name="direction"> The direction to which PTL should point. Lights up the up/down arrows</param>
           /// <param name="Dot"> each bit for every dot. 0: off, 1: on </param>
           /// <param name="Interval"> 0: normal display;  -1: blinking with default frequency; >0: set blinking frequency (by msec);  -2: turn off digit;  -3: turn off digits and lamp</param>
           public bool DisplaySeries(int gateway, int node, string disp_string, Direction direction, byte Dot, int Interval)
           {
               if (disp_string.Length > 6)
               {
                   disp_string = disp_string.Remove(0, disp_string.Length - 6);
               }
               while (disp_string.Length < 6)
               {
                   disp_string = " " + disp_string;
               }

               if (CapsAPI.AB_GW_Status(gateway) != 7)
               {
                   return false;
               }
               CapsAPI.AB_LB_SetMode(gateway, -node, 0);
               byte x = 1 * 1 + 2 * 0 + 4 * 0 + 8 * 1 + 16 * 1 + 32 * 1 + 64 * 0;
               int res = CapsAPI.AB_TAG_mode(gateway, -node, 0, x);

               CapsAPI.AB_LB5A_DspStr(gateway, -node, disp_string, (byte)direction, Dot, Interval);
               return true;
           }
           */
           /// <summary>
           /// Puts the device in stock mode and displays the number on the specified Node.
           /// </summary>
           /// <param name="gateway"> Gateway id</param>
           /// <param name="node"> Node id. (252 is for all nodes)</param>
           /// <param name="quantity"> The number to display </param>
           /// <param name="direction"> The direction to which PTL should point. Lights up the up/down arrows</param>
           /// <param name="Dot"> each bit for every dot. 0: off, 1: on </param>
           /// <param name="Interval">0: normal display;  -1: blinking with default frequency; >0: set blinking frequency (by msec);  -2: turn off digit;  -3: turn off digits and lamp</param>
           /// <param name="maxCountDigits"> Specify the maximum number of digits allowed to modify </param>
           public bool DisplayStockQuantity(int gateway, int node, string quantity, Direction direction, byte Dot, int Interval, byte maxCountDigits)
           {
               if (CapsAPI.AB_GW_Status(gateway) != 7)
               {
                   return false;
               }
               CapsAPI.AB_LB_SetMode(gateway, -node, 1);
               byte x = 1 + 0 + 4 + 8 + 16 + 32 + 0;
               int res = CapsAPI.AB_TAG_mode(gateway, -node, 0, x);
               CapsAPI.AB_TAG_CountDigit(gateway, node, maxCountDigits);
               CapsAPI.AB_LB5A_DspStr(gateway, -node, quantity, (byte)direction, Dot, Interval);
               return true;
           }
           
        public bool DisplayQuantity(int gateway, int node, int quantity)
        {
            if (CapsAPI.AB_GW_Status(gateway) != 7)
            {
                return false;
            }

            CapsAPI.AB_LCD_DspNum(gateway, -node, quantity);
            return true;
        }

        /// <summary>
        /// Displays the number on the specified Node.
        /// </summary>
        /// <param name="gateway"> Gateway id</param>
        /// <param name="node"> Node id. (252 is for all nodes)</param>
        /// <param name="disp_string"> The string to display </param>

        public bool DisplaySeries(int gateway, int node, string disp_string)
        {

            if (CapsAPI.AB_GW_Status(gateway) != 7)
            {
                return false;
            }

            CapsAPI.AB_LCD_DspMsg(gateway, -node, disp_string);
            return true;
        }


        public bool DisplayName(int gateway, int node, string disp_string)
        {
            if (CapsAPI.AB_GW_Status(gateway) != 7)
            {
                return false;
            }

            CapsAPI.AB_LCD_DspMsg(gateway, -node, disp_string);
            return true;
        }

        /// <summary>
        /// Turns off display, leds on the specified node
        /// </summary>
        /// <param name="gateway">Gateway id</param>
        /// <param name="node">Node id. (252 is for all nodes)</param>
        public bool ClearNode(int gateway, int node)
        {
            if (CapsAPI.AB_GW_Status(gateway) != 7)
            {
                return false;
            }
            CapsAPI.AB_LB5A_DspStr(gateway, -node, "", 0, 0, -3);
            return true;
        }

        /// <summary>
        /// Sets the state for LED on the confirm button. This does not turn off the display.
        /// </summary>
        /// <param name="gateway">Gateway id</param>
        /// <param name="node">Node id. (252 is for all nodes)</param>
        /// <param name="color">The LED Color</param>
        /// <param name="status">The status of LED</param>
        public bool SetLEDStatus(int gateway, int node, LEDColor color, LEDStatus status)
        {
            if (CapsAPI.AB_GW_Status(gateway) != 7)
            {
                return false;
            }
            CapsAPI.AB_LED_Status(gateway, node, (byte)color, (byte)status);
            return true;
            //CapsAPI.AB_LED_Dsp(gateway, node, (byte)status, 0);
        }

        private string Bin2Str(byte[] bufbin, int start, int cnt)
        {
            string returnValue;
            int i;
            string bufstr;
            bufstr = "";
            //UPGRADE_WARNING: µLЄkёСЄRЄ«Ґу cnt Єє№wі]ДЭ©КЎC «ц¤@¤UҐHЁъ±oёФІУёк°T: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            for (i = 0; i <= cnt - 1; i++)
            {
                bufstr = bufstr + Convert.ToChar(bufbin[i]);
            }

            returnValue = bufstr;
            return returnValue;
        }
    }

    public class ConnStateArgs : EventArgs
    {
        public bool[] status { get; set; }
    }

    public class ConfirmButtonArgs : EventArgs
    {
        public int Gateway { get; set; }
        public int Node { get; set; }
        public string Message { get; set; }
        public Direction direction { get; set; }
    }
    public class PressedArgs : EventArgs
    {
        public int Gateway { get; set; }
        public int Node { get; set; }
    }

    public enum Direction
    {
        None = 0,
        Up = 1,
        Down = 2,
        Both = 3
    }

    public enum LEDColor
    {
        Red = 0,
        Green = 1,
        Amber = 2,
        Blue = 3,
        Pink = 4,
        Cyan = 5
    }

    public enum LEDStatus
    {
        Off = 0,
        On = 1,
        Blink_2sec = 2,
        Blink_1sec = 3,
        Blink_half_sec = 4,
        Blink_quarter_sec = 5
    }
}

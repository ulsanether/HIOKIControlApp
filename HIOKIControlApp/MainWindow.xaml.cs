using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO.Ports;


using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
namespace HIOKIControlApp
{
    public class NodeViewModel
    {
        public string Title { get; set; }
        public ObservableCollection<ConnectorViewModel> Input { get; set; } = new ObservableCollection<ConnectorViewModel>();
        public ObservableCollection<ConnectorViewModel> Output { get; set; } = new ObservableCollection<ConnectorViewModel>();
    }

    public class ConnectorViewModel : INotifyPropertyChanged
    {
        private Point _anchor;
        public Point Anchor
        {
            set
            {
                _anchor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
            }
            get => _anchor;
        }

        public string Title { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ConnectionViewModel
    {
        public ConnectorViewModel Source { get; set; }
        public ConnectorViewModel Target { get; set; }
    }



    public partial class MainWindow : Window
    {
        
        private SerialPort SerialPort1 = new SerialPort();
        private string MsgBuf = string.Empty;


        public ObservableCollection<NodeViewModel> Nodes { get; } = new ObservableCollection<NodeViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
        /// <summary>
        /// 1 .명령어 커맨드 정리
        /// 2. 명령어 커맨드 노드 블럭으로 연결할 수 있게 설정
        /// 3. 명령어 커맨드 노드 블럭으로 연결되면 설정 저장되게 할것. 
        /// 4. 노드 블럭 send로 보내기. 
        /// </summary>


        private readonly Dictionary<string, string> CommandDescriptions = new()
        {
            {"*CLS", "상태 바이트 및 관련 큐를 초기화합니다."},
            {"*ESE A", "SESER(이벤트 상태 enable 레지스터) 값을 A(0~255)로 설정합니다."},
            {"*ESE?", "SESER 값을 조회합니다."},
            {"*ESR?", "SESR(이벤트 상태 레지스터) 값을 조회하고 클리어합니다."},
            {"*IDN?", "장비의 제조사, 모델명, 시리얼번호, 소프트웨어 버전을 조회합니다."},
            {"*OPC", "명령 처리 완료 시 SESR의 LSB를 설정합니다."},
            {"*OPC?", "명령 처리 완료 시 ASCII 1을 반환합니다."},
            {"*OPT?", "장비의 옵션 정보를 조회합니다."},
            {"*RST", "장비 설정을 초기화(리셋)합니다."},
            {"*STB?", "상태 바이트(STB)와 MSS 비트를 읽어옵니다."},
            {"*TST?", "ROM/RAM 체크 결과를 조회합니다."},
            {"*WAI", "처리 완료 후 다음 명령을 실행합니다."},
            {":ESE0 A", "ESER0 값을 A(0~255)로 설정합니다."},
            {":ESE0?", "ESER0 값을 조회합니다."},
            {":ESR0?", "ESR0 값을 조회합니다."},
            {":ABORT", "동작 중단(Abort)"},
            {":AUTO", "타임베이스와 전압 범위 자동 설정"},
            {":DELIMITER A$", "구분자 설정 (A$: LF, CR_LF)"},
            {":DELIMITER?", "구분자 설정값 조회"},
            {":ERROR?", "에러 번호 조회"},
            {":FEED A", "프린터 용지 피드 (A: 1~255, 내부 프린터 전용)"},
            {":FUNCTION A$", "동작 모드 선택 (A$: MEM, REC, XYC, FFT)"},
            {":FUNCTION?", "현재 동작 모드 조회"},
            {":HCOPY", "화면 하드카피(프린트)"},
            {":HEADER A$", "쿼리 응답에 헤더 프리픽스 사용 여부 설정 (A$: ON, OFF)"},
            {":HEADER?", "헤더 프리픽스 설정값 조회"},
            {":PRINT", "프린트"},
            {":REPORT", "리포트 출력"},
            {":SAVE", "저장"},
            {":START", "동작 시작"},
            {":STATUS?", "저장 상태 조회 (A: 0~127, 비트별 상태)"},
            {":STOP", "동작 정지"}
        };



        public MainWindow()
        {
            InitializeComponent();
      
            ListBox1.ItemsSource = CommandDescriptions.Keys;
            this.DataContext = this;
            var welcome = new NodeViewModel
            {
                Title = "Welcome",
                Input = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Title = "In"
                }
            },
                Output = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Title = "Out"
                }
            }
            };

            var nodify = new NodeViewModel
            {
                Title = "To Nodify",
                Input = new ObservableCollection<ConnectorViewModel>
            {
                new ConnectorViewModel
                {
                    Title = "In"
                }
            }
            };

            Nodes.Add(welcome);
            Nodes.Add(nodify);

            Connections.Add(new ConnectionViewModel
            {
                Source = welcome.Output[0],
                Target = nodify.Input[0]
            });


            PortListBox.ItemsSource = SerialPort.GetPortNames();
        }
        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PortListBox.SelectedItem is string portName)
                {
                    SerialPort1.PortName = portName;
                }
                else
                {
                    MessageBox.Show("포트를 선택하세요.");
                    return;
                }

                SerialPort1.BaudRate = 19200;
                SerialPort1.DataBits = 8;
                SerialPort1.Parity = Parity.None;
                SerialPort1.StopBits = StopBits.One;
                SerialPort1.Handshake = Handshake.None;

                SerialPort1.Open();
                Button1.IsEnabled = false;
                Button2.IsEnabled = true;
                Button3.IsEnabled = true;
                TextBox2.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SerialPort1.Close();
                Button1.IsEnabled = true;
                Button2.IsEnabled = false;
                Button3.IsEnabled = false;
                TextBox2.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            SendMsg(":HEADER OFF");
            SendMsg(":FUNCTION MEM");
            SendMsg(":CONFIGURE:SHOT 100");
            SendMsg(":CONFIGURE:TDIV 5E-3");
            SendMsg(":TRIGGER:MODE SINGLE");
            SendMsg(":START");

            SendQueryMsg(":STOP;*OPC?");
            SendQueryMsg(":MEMORY:MAXPOINT?");

            TextBox2.Text = "";

            if (MsgBuf == "0")
            {
                TextBox2.Text = "No data";
                return;
            }

            SendMsg(":MEMORY:POINT CH1,0");

            for (int i = 0; i <= 100; i++)
            {
                SendQueryMsg(":MEMORY:VDATA? 1");
                if (double.TryParse(MsgBuf, out double data))
                {
                    TextBox2.AppendText($"{data:E}\r\n");
                }
            }
        }

        private void SendMsg(string strMsg)
        {
            try
            {
                strMsg += "\r\n";
                SerialPort1.WriteLine(strMsg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendQueryMsg(string strMsg)
        {
            try
            {
                strMsg += "\r\n";
                SerialPort1.WriteLine(strMsg);
                int check;
                MsgBuf = string.Empty;
                do
                {
                    check = SerialPort1.ReadByte();
                    if ((char)check == '\n')
                        break;
                    else if ((char)check == '\r')
                        continue;
                    else
                        MsgBuf += (char)check;
                } while (true);
                MsgBuf += "\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox1.SelectedItem is string cmd && CommandDescriptions.ContainsKey(cmd))
            {
                Label1.Content = CommandDescriptions[cmd];
            }
            else
            {
                Label1.Content = "";
            }
        }

        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            SendMsg(":START");
        }

        private void Button5_Click(object sender, RoutedEventArgs e)
        {
            SendMsg(":STOP");
        }

        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox1.SelectedItem is string cmd)
            {
                SendMsg(cmd);
            }
            else
            {
                MessageBox.Show("명령어를 선택하세요.");
            }
        }

   
    }
}

using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using Octokit;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Label = System.Windows.Forms.Label;

namespace GersangStation {
    public partial class Form1 : MaterialForm {
        private const int WM_ACTIVATEAPP = 0x001C;

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            if (WindowState == FormWindowState.Minimized && m.Msg == WM_ACTIVATEAPP && m.WParam == IntPtr.Zero) {
                BringToFront();
            }
        }

        public enum State {
            LoggedIn, LoginOther, None
        }

        public enum Client {
            None = 0,
            Client1 = 1,
            Client2 = 2,
            Client3 = 3
        }

        private State currentState = State.None;
        private Client currentClient = Client.None;

        private const string url_main = "http://www.gersang.co.kr/main/index.gs?";
        private const string url_logout = "http://www.gersang.co.kr/member/logoutProc.gs";
        private const string url_installStarter = "http://akgersang.xdn.kinxcdn.com//PatchFile/Gersang_Web/GersangStarterSetup.exe";
        private const string url_search = "https://search.naver.com/search.naver?&query=거상";
        private const string url_search_gersang = "http://www.gersang.co.kr/main.gs";

        private const string url_main_vsn = @"http://akgersang.xdn.kinxcdn.com/Gersang/Patch/Gersang_Server/" + @"Client_Patch_File/" + @"Online/vsn.dat.gsz";
        private const string url_test_vsn = @"http://akgersang.xdn.kinxcdn.com/Gersang/Patch/Test_Server/" + @"Client_Patch_File/" + @"Online/vsn.dat.gsz";

        private const string url_release = "https://github.com/byungmeo/GersangStation/releases/latest";

        private bool isWebFunctionDeactivated = false;
        private bool isSearch = false;
        private bool isGameStartLogin = false;
        private string previousUrl = "";

        WebView2? webView_main = null;

        public Form1() {
            InitializeComponent();

            // Initialize MaterialSkinManager
            var materialSkinManager = MaterialSkinManager.Instance;

            // Set this to false to disable backcolor enforcing on non-materialSkin components
            // This HAS to be set before the AddFormToManage()
            materialSkinManager.EnforceBackcolorOnAllComponents = false;

            // MaterialSkinManager properties
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private async void Form1_Load(object sender, EventArgs e) {
            webView_main = new WebView2() {
                Visible = true,
                Dock = DockStyle.Fill,
                Source = new Uri("https://www.gersang.co.kr/main/index.gs")
            };

            try {
                //webView_main.CoreWebView2InitializationCompleted += WebView_main_CoreWebView2InitializationCompleted;
                await webView_main.EnsureCoreWebView2Async(null); //무조건 CoreWebView2InitializationCompleted 리스너를 부착 후 실행해야 함.
                webView_main.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                webView_main.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false; //Alert 등의 메시지창이 뜨지않고 ScriptDialogOpening 이벤트를 통해 제어할 수 있도록 합니다.
                webView_main.CoreWebView2.ScriptDialogOpening += CoreWebView2_ScriptDialogOpening;
            } catch (WebView2RuntimeNotFoundException ex) {
                Trace.WriteLine(ex.StackTrace);
                DialogResult dr = MessageBox.Show("다클라 스테이션을 이용하기 위해선\nWebView2 런타임을 반드시 설치하셔야 합니다.\n설치 하시겠습니까? (설치 링크에 자동으로 접속합니다.)", "런타임 설치 필요", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dr == DialogResult.Yes) {
                    Process.Start(new ProcessStartInfo("https://go.microsoft.com/fwlink/p/?LinkId=2124703") { UseShellExecute = true });
                }
                System.Windows.Forms.Application.Exit();
                return;
            }

            webView_main.NavigationStarting += webView_main_NavigationStarting;
            webView_main.NavigationCompleted += webView_main_NavigationCompleted;

            webView_main.Source = new Uri("https://www.gersang.co.kr/main/index.gs");

            LoadComponent();
        }

        private void LoadComponent() {
            ConfigManager.Validation();
            LoadCheckBox();
            LoadRadioButton();
            LoadAccountComboBox();
            LoadShortcut();
            SetToolTip();
            CheckAccount();
            CheckProgramUpdate();
        }

        private void SetToolTip() {
            toolTip1.Active = true;
            /**
             * <-- 메인화면 -->
             */
            toolTip1.SetToolTip(button_tray, "트레이에 숨기기");
            toolTip1.SetToolTip(radio_preset_1, "1번 세팅");
            toolTip1.SetToolTip(radio_preset_2, "2번 세팅");
            toolTip1.SetToolTip(radio_preset_3, "3번 세팅");
            toolTip1.SetToolTip(radio_preset_4, "4번 세팅");
            toolTip1.SetToolTip(materialCheckbox_testServer, "활성화 시 테스트 서버로 실행합니다.\n(설치 별도)");
            toolTip1.SetToolTip(materialButton_debugging, "작동하는 브라우저 직접 보기");
            toolTip1.SetToolTip(materialComboBox_account_1, "본클라 계정 선택");
            toolTip1.SetToolTip(materialComboBox_account_2, "2클라 계정 선택");
            toolTip1.SetToolTip(materialComboBox_account_3, "3클라 계정 선택");
            toolTip1.SetToolTip(materialSwitch_login_1, "본클라 홈페이지 로그인");
            toolTip1.SetToolTip(materialSwitch_login_2, "2클라 홈페이지 로그인");
            toolTip1.SetToolTip(materialSwitch_login_3, "3클라 홈페이지 로그인");
            toolTip1.SetToolTip(materialButton_search_1, "본클라 검색보상 수령");
            toolTip1.SetToolTip(materialButton_search_2, "2클라 검색보상 수령");
            toolTip1.SetToolTip(materialButton_search_3, "3클라 검색보상 수령");
            toolTip1.SetToolTip(materialButton_start_1, "본클라 게임 실행");
            toolTip1.SetToolTip(materialButton_start_2, "2클라 게임 실행");
            toolTip1.SetToolTip(materialButton_start_3, "3클라 게임 실행");
            string shortcut_1 = ConfigManager.getConfig("shortcut_1");
            if (shortcut_1 == "") shortcut_1 = "링크가 설정되지 않았습니다.";
            string shortcut_2 = ConfigManager.getConfig("shortcut_2");
            if (shortcut_2 == "") shortcut_2 = "링크가 설정되지 않았습니다.";
            string shortcut_3 = ConfigManager.getConfig("shortcut_3");
            if (shortcut_3 == "") shortcut_3 = "링크가 설정되지 않았습니다.";
            string shortcut_4 = ConfigManager.getConfig("shortcut_4");
            if (shortcut_4 == "") shortcut_4 = "링크가 설정되지 않았습니다.";
            toolTip1.SetToolTip(materialButton_shortcut_1, shortcut_1);
            toolTip1.SetToolTip(materialButton_shortcut_2, shortcut_2);
            toolTip1.SetToolTip(materialButton_shortcut_3, shortcut_3);
            toolTip1.SetToolTip(materialButton_shortcut_4, shortcut_4);
        }

        private async void CheckProgramUpdate() {
            //버전 업데이트 시 Properties -> AssemblyInfo.cs 의 AssemblyVersion과 AssemblyFileVersion을 바꿔주세요.
            string version_current = Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 5);
            Trace.WriteLine(version_current);

            string version_latest;

            try {
                //깃허브에서 모든 릴리즈 정보를 받아옵니다.
                GitHubClient client = new GitHubClient(new ProductHeaderValue("Byungmeo"));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("byungmeo", "GersangStation");
                version_latest = releases[0].TagName;
                label_version_current.Text = label_version_current.Text.Replace("00000", version_current);
                label_version_latest.Text = label_version_latest.Text.Replace("00000", version_latest);

                //깃허브에 게시된 마지막 버전과 현재 버전을 초기화 합니다.
                //Version latestGitHubVersion = new Version(releases[0].TagName);
                Version latestGitHubVersion = new Version(version_latest);
                Version localVersion = new Version(version_current);
                Trace.WriteLine("깃허브에 마지막으로 게시된 버전 : " + latestGitHubVersion);
                Trace.WriteLine("현재 프로젝트 버전 : " + localVersion);

                //버전 비교
                int versionComparison = localVersion.CompareTo(latestGitHubVersion);
                if (versionComparison < 0) {
                    Trace.WriteLine("구버전입니다! 업데이트 메시지박스를 출력합니다!");

                    DialogResult dr = MessageBox.Show(releases[0].Body + "\n\n업데이트 하시겠습니까? (GitHub 접속)",
                        "업데이트 안내", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dr == DialogResult.Yes) {
                        Process.Start(new ProcessStartInfo(url_release) { UseShellExecute = true });
                    }
                } else if (versionComparison > 0) {
                    Trace.WriteLine("깃허브에 릴리즈된 버전보다 최신입니다!");
                } else {
                    Trace.WriteLine("현재 버전은 최신버전입니다!");
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "프로그램 업데이트 확인 도중 에러가 발생하였습니다.\n에러 메시지를 캡쳐하고, 문의 부탁드립니다.", "업데이트 확인 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show(this, "에러 메시지1 : \n" + ex.Message);
                MessageBox.Show(this, "에러 메시지2 : \n" + ex.ToString());
                Trace.WriteLine(ex.Message);
            }
        }

        private void CheckAccount() {
            if (materialComboBox_account_1.Items.Count <= 1) {
                DialogResult dr = MessageBox.Show("현재 저장된 계정이 하나도 없습니다.\n계정 설정 화면으로 이동하시겠습니까?", "계정 없음", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dr == DialogResult.OK) {
                    OpenAccountSettingDialog();
                }
            }
        }

        private void LoadCheckBox() {
            this.materialCheckbox_testServer.Checked = bool.Parse(ConfigManager.getConfig("is_test_server"));
        }

        private void CoreWebView2_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e) {
            if (sender != null) e.NewWindow = (CoreWebView2)sender;
            //e.Handled = true;
        }

        private async void CoreWebView2_ScriptDialogOpening(object? sender, CoreWebView2ScriptDialogOpeningEventArgs e) {
            string message = e.Message;
            Trace.WriteLine(message);
            Trace.WriteLine("대화창 종류 : " + e.Kind);

            if (e.Kind == CoreWebView2ScriptDialogKind.Confirm) {
                Trace.WriteLine("선택지가 있는 대화상자 판정");
                DialogResult dr = DialogResult.None;
                var task = Task.Run(() => {
                    dr = MessageBox.Show(message, "선택", MessageBoxButtons.YesNo, MessageBoxIcon.Information, 
                        MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                });
                await task;
                if (dr == DialogResult.Yes) {
                    e.Accept();
                }

                return;
            }

            this.BeginInvoke(async () => {
                //message가 정확히 "5초 후에 재로그인 가능합니다." 일 경우, 사용자가 로그인 실패 후 5초 이내에 로그인을 시도한 경우입니다.
                if (message.Equals("5초 후에 재로그인 가능합니다.")) {
                    Trace.WriteLine("로그인 실패 후 5초 안에 로그인 시도 판정");
                    MessageBox.Show("아직 5초가 지나지 않았습니다. 5초 후에 다시 로그인을 시도해주세요.");
                    currentState = State.None;
                    currentClient = Client.None;
                }

                //otp 인증번호가 틀릴 시 다시 입력하도록 합니다.
                else if (message.Contains("인증번호가 다릅니다")) {
                    Trace.WriteLine("잘못된 OTP 코드 입력 판정");
                    MessageBox.Show("잘못된 OTP 코드를 입력하였습니다. 다시 입력해주세요.");
                    string? otpCode = showDialogOtp();

                    Trace.WriteLine("otpCode : " + otpCode);
                    if (otpCode == null) {
                        MessageBox.Show("OTP 코드를 입력하지 않았습니다.");
                    } else {
                        await webView_main.ExecuteScriptAsync("document.getElementById('GSotpNo').value = '" + otpCode + "'");
                        await webView_main.ExecuteScriptAsync("document.getElementById('btn_Send').click()");
                    }
                }

                //검색 보상 지급 메시지
                else if (message.Contains("아이템이 지급되었습니다.") || message.Contains("이미 아이템을 수령하셨습니다.") || message.Contains("참여 시간이 아닙니다.")) {
                    if (MessageBox.Show(message) == DialogResult.OK) { 
                        webView_main.CoreWebView2.Navigate(url_main); 
                    }
                }

                //아이디 또는 비밀번호가 틀린 경우입니다.
                else if (message.Contains("아이디 또는 비밀번호 오류")) {
                    currentState = State.None;
                    currentClient = Client.None;
                    Trace.WriteLine("로그인 실패 판정");
                    MessageBox.Show("로그인에 실패하였습니다. 5초후에 다시 로그인 해주세요.");
                } 
                
                else {
                    Trace.WriteLine("예외로 처리되지 않은 메시지 판정");
                    MessageBox.Show(message);
                }
            });
        }
        private void webView_main_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e) {
            Trace.WriteLine("NavigationStarting : " + e.Uri.ToString());
            Trace.WriteLine("NavigationStarting Previous URL : " + webView_main.Source);
            previousUrl = webView_main.CoreWebView2.Source;
            deactivateWebSideFunction();
        }

        private void webView_main_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e) {
            /*
                1. 스위치로 로그인 하는 경우
                2. 스위치로 로그아웃 하는 경우
                3. 스위치로 로그아웃 -> 로그인 하는 경우 (2)
                4. (로그인 된 상태에서) 검색버튼으로 검색보상 수령
                5. (로그인 된 상태에서) 실행버튼으로 실행
            */

            if (sender == null) {
                this.BeginInvoke(() => { MessageBox.Show("NavigationFailed : sender is NULL"); });
                return;
            }

            if (!e.IsSuccess) {
                deactivateWebSideFunction(); //WebView2 활용 기능 비활성화 처리
                this.BeginInvoke(() => { handleWebError(e.WebErrorStatus); });
                return;
            } else { if (isWebFunctionDeactivated) { activateWebSideFunction(); } } //WebView2 활용 기능이 비활성화 상태인 경우 활성화 처리

            string? url = ((WebView2)sender).Source.ToString();
            Trace.WriteLine("NavigationCompleted : " + url);

            if (url.Contains("pw_reset.gs")) {
                doPwReset();
                return;
            }

            if (url.Contains("otp.gs")) {
                this.BeginInvoke(() => {
                    string? otpCode = showDialogOtp();

                    if (otpCode == null) { MessageBox.Show("OTP 코드를 입력하지 않았습니다."); } 
                    else { doOtpInput(otpCode); }
                });
                return;
            }

            if (url.Contains("search.naver")) {
                doNavigateGersangSite();
                return;
            }

            if (url.Contains("attendance")) {
                if (isSearch) { doGetEventItem(); }
                //else { webView_main.CoreWebView2.Navigate(url_main); }
                return;
            }

            if (url.Contains("main/index.gs")) {
                if(isSearch && previousUrl.Contains("search.naver")) {
                    doNavigateAttendancePage();
                    return;
                }

                if(currentState == State.LoginOther) { doLoginOther(); } 
                else if (currentState == State.LoggedIn) { doCheckLogin(); }
                return;
            }
        }

        private async void doGetEventItem() {
            isSearch = false;

            /* 검색보상 수령 시간대별 인자 값
            1-> 00:05 ~05:55
            2-> 06:05 ~11:55
            3-> 12:05 ~17:55
            4-> 18:05 ~23:55
            */
            int arg; //event_Search_Use 스크립트 실행 인자

            int koreaHour = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Korea Standard Time").Hour;
            if (koreaHour >= 0 && koreaHour <= 5) { arg = 1; } 
            else if (koreaHour >= 6 && koreaHour <= 11) { arg = 2; } 
            else if (koreaHour >= 12 && koreaHour <= 17) { arg = 3; } 
            else { arg = 4; }

            await webView_main.ExecuteScriptAsync("event_Search_Use(" + arg + ");");
        }

        private async void doNavigateAttendancePage() {
            await webView_main.ExecuteScriptAsync(@"document.querySelector(""a[href *= '" + "attendance" + @"']"").click();");
        }

        private async void doNavigateGersangSite() {
            await webView_main.ExecuteScriptAsync(@"document.querySelector(""a[href = '" + url_search_gersang + @"']"").click();");
        }

        private void deactivateWebSideFunction() {
            //materialSwitch_login_1.Enabled = false;
            //materialSwitch_login_2.Enabled = false;
            //materialSwitch_login_3.Enabled = false;
            materialButton_search_1.Enabled = false;
            materialButton_search_2.Enabled = false;
            materialButton_search_3.Enabled = false;
            materialButton_start_1.Enabled = false;
            materialButton_start_2.Enabled = false;
            materialButton_start_3.Enabled = false;
            materialButton_shortcut_1.Enabled = false;
            materialButton_shortcut_2.Enabled = false;
            materialButton_shortcut_3.Enabled = false;
            materialButton_shortcut_4.Enabled = false;
            Trace.WriteLine("웹뷰 관련 기능들이 비활성화 되었습니다.");
            isWebFunctionDeactivated = true;
        }

        private void activateWebSideFunction() {
            //materialSwitch_login_1.Enabled = true;
            //materialSwitch_login_2.Enabled = true;
            //materialSwitch_login_3.Enabled = true;
            materialButton_search_1.Enabled = true;
            materialButton_search_2.Enabled = true;
            materialButton_search_3.Enabled = true;
            materialButton_start_1.Enabled = true;
            materialButton_start_2.Enabled = true;
            materialButton_start_3.Enabled = true;
            materialButton_shortcut_1.Enabled = true;
            materialButton_shortcut_2.Enabled = true;
            materialButton_shortcut_3.Enabled = true;
            materialButton_shortcut_4.Enabled = true;
            Trace.WriteLine("웹뷰 관련 기능들이 다시 활성화 되었습니다.");
            isWebFunctionDeactivated = false;
        }

        private void handleWebError(CoreWebView2WebErrorStatus webErrorStatus) {
            Trace.WriteLine("NavigationFailed - WebErrorStatus : " + webErrorStatus);
            Trace.WriteLine("NavigationFailed - DocumentTitle : " + webView_main.CoreWebView2.DocumentTitle);

            //작업이 취소되었음을 나타냅니다. (MS DOC)
            if (webErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) { return; }

            //연결이 중지되었음을 나타냅니다. (MS DOC)
            if (webErrorStatus == CoreWebView2WebErrorStatus.ConnectionAborted) { return; }

            this.BeginInvoke(() => {
                //알 수 없는 오류가 발생했음을 나타냅니다. (MS DOC)
                if (webErrorStatus == CoreWebView2WebErrorStatus.Unknown) {
                    string title = webView_main.CoreWebView2.DocumentTitle;
                    if (title != null) {
                        if (title.Contains("점검")) {
                            MessageBox.Show("현재 거상 홈페이지가 점검 중입니다.\n점검이 끝난 후에 주요 기능 이용이 가능합니다.");
                            return;
                        }
                        MessageBox.Show("알 수 없는 오류로 인해 거상 홈페이지 접속에 실패하였습니다.\nDocumentTitle : " + webView_main.CoreWebView2.DocumentTitle);
                    } else {
                        MessageBox.Show("알 수 없는 오류로 인해 거상 홈페이지 접속에 실패하였습니다.\nDocumentTitle : NULL");
                    }
                    return;
                }

                //인터넷 연결이 끊어졌음을 나타냅니다. (MS DOC)
                if (webErrorStatus == CoreWebView2WebErrorStatus.Disconnected) {
                    MessageBox.Show("거상 홈페이지 접속에 실패하였습니다.\n인터넷 연결을 확인 해주세요.");
                    return;
                }

                MessageBox.Show("거상 홈페이지 접속에 실패하였습니다.\n문제 원인 : " + webErrorStatus + "\n문의 바랍니다.");
                return;
            });
        }

        private async void doCheckLogin() {
            var logout_btn = await webView_main.ExecuteScriptAsync(@"document.querySelector(""a[href = '" + "/member/logoutProc.gs" + @"']"")");
            if (logout_btn != null) {
                Trace.WriteLine("체크하였더니 로그인이 되어있음.");
                switch (currentClient) {
                    case Client.Client1:
                        materialSwitch_login_1.CheckState = CheckState.Checked;
                        break;
                    case Client.Client2:
                        materialSwitch_login_2.CheckState = CheckState.Checked;
                        break;
                    case Client.Client3:
                        materialSwitch_login_3.CheckState = CheckState.Checked;
                        break;
                    default:
                        break;
                }
            } else {
                Trace.WriteLine("체크하였더니 로그인이 안되어있는 상태.");
                currentState = State.None;
                materialSwitch_login_1.CheckState = CheckState.Unchecked;
                materialSwitch_login_2.CheckState = CheckState.Unchecked;
                materialSwitch_login_3.CheckState = CheckState.Unchecked;
            }

            if (true == isSearch) { webView_main.CoreWebView2.Navigate(url_search); } //네이버 검색
            else if (true == isGameStartLogin) { GameStart(); }
        }

        private async void doLoginOther() {
            var button_login = await webView_main.ExecuteScriptAsync("document.getElementById('btn_Login')");
            var textBox_id = await webView_main.ExecuteScriptAsync("document.getElementById('GSuserID')");
            var textBox_pw = await webView_main.ExecuteScriptAsync("document.getElementById('GSuserPW')");

            string? tag = null;
            string[] account;
            switch (currentClient) {
                case Client.Client1:
                    tag = materialSwitch_login_1.Tag.ToString();
                    break;
                case Client.Client2:
                    tag = materialSwitch_login_2.Tag.ToString();
                    break;
                case Client.Client3:
                    tag = materialSwitch_login_3.Tag.ToString();
                    break;
                default:
                    break;
            }

            if (tag != null) { account = tag.Split(';'); } else { return; }

            if (button_login != null && textBox_id != null && textBox_pw != null) { Login(account[0], account[1]); }
        }

        private async void doOtpInput(string otpCode) {
            await webView_main.ExecuteScriptAsync("document.getElementById('GSotpNo').value = '" + otpCode + "'");
            await webView_main.ExecuteScriptAsync("document.getElementById('btn_Send').click()");
        }

        private async void doPwReset() {
            await webView_main.ExecuteScriptAsync(@"document.querySelector(""a[href = '" + url_main + @"']"").click()");
        }

        private string? showDialogOtp() {
            Form backgroundForm = new Form() {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = .50d,
                BackColor = Color.Black,
                Location = this.Location,
                Size = this.Size,
                ShowInTaskbar = false,
                Owner = this
            };
            backgroundForm.Show();

            MaterialForm dialog_otp = new MaterialForm() {
                FormStyle = FormStyles.ActionBar_40,
                Sizable = false,
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(202, 136),
                Text = "OTP 입력",
                MaximizeBox = false,
                MinimizeBox = false,
                TopMost = true,
                ShowInTaskbar = false,
                Owner = this
            };

            MaterialTextBox2 textBox_otp = new MaterialTextBox2() {
                MaxLength = 8,
                Location = new Point(17, 82),
                Size = new Size(111, 36),
                UseTallSize = false
            };
            dialog_otp.Controls.Add(textBox_otp);

            MaterialButton button_confirm = new MaterialButton() {
                Text = "확인",
                Location = new Point(135, 82),
                AutoSize = false,
                Size = new Size(50, 36)
            };
            button_confirm.Click += (sender, e) => {
                if(textBox_otp.Text.Length != 8) {
                    MessageBox.Show("OTP 코드는 8자리 입니다.\n다시 입력해주세요.");
                    return;
                }

                if(!Regex.IsMatch(textBox_otp.Text, @"^[0-9]+$")) {
                    MessageBox.Show("OTP 코드는 숫자로만 이루어져야 합니다.\n다시 입력해주세요.");
                    return;
                }

                dialog_otp.DialogResult = DialogResult.OK;
            };

            dialog_otp.Controls.Add(button_confirm);
            dialog_otp.AcceptButton = button_confirm; //엔터 버튼을 누르면 이 버튼을 클릭합니다.

            if (dialog_otp.ShowDialog() == DialogResult.OK) {
                backgroundForm.Dispose();
                return textBox_otp.Text;
            } else {
                backgroundForm.Dispose();
                return null;
            }
        }

        private void materialButton_start_Click(object sender, EventArgs e) {
            MaterialButton startButton = (MaterialButton)sender;
            MaterialSwitch? loginSwitch = null;

            if (startButton.Equals(materialButton_start_1)) { loginSwitch = materialSwitch_login_1; } 
            else if (startButton.Equals(materialButton_start_2)) { loginSwitch = materialSwitch_login_2; } 
            else { loginSwitch = materialSwitch_login_3; }

            if (true == loginSwitch.Checked) { GameStart(); } 
            else { 
                isGameStartLogin = true; 
                SwitchClick(loginSwitch); 
            }
        }

        private async void GameStart() {
            isGameStartLogin = false;

            string configKey;
            string regKey;
            string server;
            string url_vsn;
            if (materialCheckbox_testServer.Checked) {
                configKey = "client_path_test_";
                regKey = "TestPath";
                server = "test";
                url_vsn = url_test_vsn;
            } 
            else { 
                configKey = "client_path_";
                regKey = "InstallPath";
                server = "main";
                url_vsn = url_main_vsn;
            }

            string client_path;
            switch (currentClient) {
                case Client.Client1:
                    client_path = ConfigManager.getConfig(configKey + '1');
                    break;
                case Client.Client2:
                    client_path = ConfigManager.getConfig(configKey + '2');
                    break;
                case Client.Client3:
                    client_path = ConfigManager.getConfig(configKey + '3');
                    break;
                default:
                    client_path = "";
                    break;
            }

            if (client_path == "") {
                DialogResult dr = MessageBox.Show(this, "클라이언트 경로가 지정되어 있지 않습니다.\n설정 창으로 이동하시겠습니까?", "경로 미지정", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dr == DialogResult.OK) { OpenClientSettingDialog(); }
                return;
            }

            if (false == ValidationPath(client_path, server)) return;
            
            string version_current = Form_Patcher.GetCurrentVersion(this, ConfigManager.getConfig(configKey + '1'));
            string version_latest = Form_Patcher.GetLatestVersion_Safe(this, url_vsn);
            if (version_current != version_latest) {
                DialogResult dr = DialogResult.No;
                bool update = false;
                if (!bool.Parse(ConfigManager.getConfig("is_auto_update"))) {
                    dr = MessageBox.Show(this, "거상 업데이트가 가능합니다! (" + version_current + "->" + version_latest + ")\n프로그램 기능을 사용하여 업데이트 하시겠습니까?\n거상 스테이션은 공식 패치 프로그램보다\n더 빠르게 업데이트 가능합니다.",
                        "거상 패치", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                } else {
                    update = true;
                }

                if (dr == DialogResult.Yes || update == true) {
                    this.BeginInvoke(() => {
                        Form backgroundForm = InitBackgroundForm(this);

                        bool isTest = (server == "test") ? true : false;
                        Form_Patcher form_Patcher = new Form_Patcher(isTest) {
                            Owner = this
                        };

                        try {
                            backgroundForm.Show();
                            form_Patcher.ShowDialog();
                        } catch (Exception ex) {
                            Trace.WriteLine(ex.StackTrace);
                        } finally {
                            backgroundForm.Dispose();
                        }
                    });
                    return;
                }
                Trace.WriteLine("일반 패치를 선택하였습니다.");
            }
            
            try {
                //해당 클라이언트의 경로를 레지스트리에 등록시킵니다.
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\JOYON\Gersang\Korean", RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (registryKey != null) {
                    registryKey.SetValue(regKey, client_path);
                    registryKey.Close();
                }
            } catch (Exception ex) {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            }

            string? value = null;
            try {
                //Gersang Game Starter 경로가 저장되어있는 레지스트리 경로에 접근하여 경로를 얻습니다.
                value = Registry.ClassesRoot.OpenSubKey("Gersang").OpenSubKey("shell").OpenSubKey("open").OpenSubKey("command").GetValue("").ToString();
            } catch (Exception ex) {
                Trace.WriteLine(ex.Message);
                Trace.WriteLine(ex.StackTrace);
                value = null;
            }

            string starterPath; //거상 스타터의 경로를 저장
            //GameStarter 경로를 찾을 수 없는 경우, Starter 다운로드 안내창을 띄웁니다.
            if (value == null) {
                OpenGersangStarterInstallDialog();
                return;
            } else {
                starterPath = value.Replace(@"""", String.Empty);
            }

            //레지스트리에 저장된 GameStarter의 경로에 실제로 Starter가 설치되어 있지 않은 경우, 다운로드 안내창을 띄웁니다.
            if (!File.Exists(starterPath)) {
                OpenGersangStarterInstallDialog();
                return;
            } else {
                await webView_main.ExecuteScriptAsync(@"startRetry = setTimeout(""socketStart('" + server + @"')"", 2000);"); //소켓을 엽니다.
                Process.Start(value.ToString()); //거상 스타터를 실행합니다.
            }
        }

        private bool ValidationPath(string client_path, string server) {
            string iniName;
            if (server == "main") { iniName = "GerSangKR.ini"; } 
            else { iniName = "GerSangKRTest.ini"; }

            if (!File.Exists(client_path + "\\" + "Gersang.exe")) {
                this.BeginInvoke(() => {
                    MessageBox.Show(this, "거상 경로를 다시 설정해주세요.\n원인 : Gersang.exe 파일이 없습니다.", "실행 불가", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }); 
                return false;
            }

            if (!File.Exists(client_path + "\\" + iniName)) {
                string message;
                if (server == "main") { message = "본섭 경로가 아닙니다."; }
                else { message = "테섭 경로가 아닙니다."; }
                this.BeginInvoke(() => {
                    MessageBox.Show(this, "거상 경로를 다시 설정해주세요.\n원인 : " + message, "실행 불가", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                });
                return false;
            }

            return true;
        }

        private void OpenGersangStarterInstallDialog() {
            if (MessageBox.Show("GersangGameStarter가 설치되어 있지 않습니다.\n다운로드 받으시겠습니까? (거상 공식 다운로드 링크입니다)", "게임 실행 실패", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) {
                try {
                    Process.Start(new ProcessStartInfo(url_installStarter) { UseShellExecute = true });
                } catch (Exception ex2) {
                    Trace.WriteLine(ex2.Message);
                    Trace.WriteLine(ex2.StackTrace);
                }
            } else {
                MessageBox.Show("거상 홈페이지 -> 자료실 -> 클라이언트 -> GersangGameStarter 수동 설치\n위 경로에서 거상 실행기를 다운로드 받으셔야 게임 실행이 가능합니다.", "게임 실행 실패", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void materialSwitch_login_Click(object sender, EventArgs e) {
            SwitchClick((MaterialSwitch)sender);
        }

        private void SwitchClick(MaterialSwitch sender) {
            if (isWebFunctionDeactivated) {
                Trace.WriteLine("웹 로딩 중 스위치 작동이 불가능 합니다.");
                return;
            }

            MaterialSwitch materialSwitch = sender;
            string name = materialSwitch.Name;
            currentClient = (Client)Byte.Parse(name.Substring(name.Length - 1));

            //로그아웃
            if (materialSwitch.Checked) {
                Trace.WriteLine("로그아웃 합니다.");
                webView_main.CoreWebView2.Navigate(url_logout);
                materialSwitch.CheckState = CheckState.Unchecked;
                currentState = State.None;
                currentClient = Client.None;
                return;
            }

            if (this.currentState == State.LoggedIn) {
                Trace.WriteLine("다른 계정에 로그인 하기 위해 로그아웃 합니다.");

                materialSwitch_login_1.CheckState = CheckState.Unchecked;
                materialSwitch_login_2.CheckState = CheckState.Unchecked;
                materialSwitch_login_3.CheckState = CheckState.Unchecked;

                currentState = State.LoginOther;

                webView_main.CoreWebView2.Navigate(url_logout);
                return;
            }

            if (materialSwitch.Tag == null || materialSwitch.Tag.ToString().Length == 0 || materialSwitch.Tag.ToString().Contains("선택안함")) {
                MessageBox.Show("로그인 할 계정이 선택되지 않았습니다.");
                return;
            }

            string[] account = materialSwitch.Tag.ToString().Split(';');
            Login(account[0], account[1]);
        }

        private void materialButton_shortcut_Click(object sender, EventArgs e) {
            MaterialButton button = (MaterialButton)sender;
            string? url = ConfigManager.getConfig("shortcut_" + button.Name.Substring(button.Name.Length - 1, 1));

            if (url == null || url.Equals("")) {
                DialogResult dr = MessageBox.Show("나만의 바로가기 링크가 설정되어 있지 않습니다.\n설정화면으로 이동하시겠습니까?", "바로가기 미설정", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dr == DialogResult.OK) { OpenShortcuttSettingDialog(); }
                return;
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                MessageBox.Show("잘못된 링크 형식 입니다. 다시 설정해주세요.");
                return;
            }

            MaterialForm form = new MaterialForm() {
                Size = new Size(1500, 1000),
                FormStyle = FormStyles.ActionBar_None,
                StartPosition = FormStartPosition.CenterParent
            };

            form.Controls.Add(webView_main);
            form.Load += (sender, e) => {
                webView_main.CoreWebView2.Navigate(ConfigManager.getConfig("shortcut_" + button.Name.Substring(button.Name.Length - 1, 1)));
            };
            form.FormClosed += (sender, e) => {
                webView_main.CoreWebView2.Navigate(url_main);
                form.Controls.Clear();
            };
            form.ShowDialog();
        }

        private void materialComboBox_account_SelectedIndexChanged(object sender, EventArgs e) {
            MaterialComboBox comboBox = (MaterialComboBox)sender;

            if (Byte.Parse(comboBox.Name.Substring(comboBox.Name.Length - 1, 1)) == (byte)currentClient && currentState == State.LoggedIn) {
                Trace.WriteLine("현재 로그인한 클라이언트의 계정을 변경하였으므로, 로그아웃 합니다.");
                webView_main.CoreWebView2.Navigate(url_logout);
                materialSwitch_login_1.CheckState = CheckState.Unchecked;
                materialSwitch_login_2.CheckState = CheckState.Unchecked;
                materialSwitch_login_3.CheckState = CheckState.Unchecked;
                currentState = State.None;
                currentClient = Client.None;
                return;
            }

            string id = ConfigManager.getKeyByValue(comboBox.Text).Replace("_nickname", string.Empty);
            if (id == "") id = comboBox.Text;
            string switchTag;
            Trace.WriteLine(id);
            if(id.Contains("선택안함")) { switchTag = id; }
            else { switchTag = id + ";" + ConfigManager.getConfig(id); }

            byte current_preset = Byte.Parse(ConfigManager.getConfig("current_preset"));
            int[] temp = Array.ConvertAll(ConfigManager.getConfig("current_comboBox_index_preset_" + current_preset).Split(';'), s => int.Parse(s));

            if (comboBox.Equals(materialComboBox_account_1)) { 
                materialSwitch_login_1.Tag = switchTag;
                temp[0] = comboBox.SelectedIndex;
            } else if(comboBox.Equals(materialComboBox_account_2)) { 
                materialSwitch_login_2.Tag = switchTag;
                temp[1] = comboBox.SelectedIndex;
            } else { 
                materialSwitch_login_3.Tag = switchTag;
                temp[2] = comboBox.SelectedIndex;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var item in temp) {
               sb.Append(item.ToString() + ';');
            }
            sb.Remove(sb.Length - 1, 1);
            ConfigManager.setConfig("current_comboBox_index_preset_" + current_preset, sb.ToString());
        }

        private void LoadAccountComboBox() {
            materialComboBox_account_1.Items.Clear();
            materialComboBox_account_2.Items.Clear();
            materialComboBox_account_3.Items.Clear();

            string temp = ConfigManager.getConfig("account_list");
            string[] accountList;
            if (temp.Length != 0) {
                accountList = temp.Remove(temp.Length - 1, 1).Split(';');
            } else {
                accountList = new string[0];
            }

            materialComboBox_account_1.Items.Add("선택안함");
            materialComboBox_account_2.Items.Add("선택안함");
            materialComboBox_account_3.Items.Add("선택안함");

            foreach (var item in accountList) {
                string id = ConfigManager.getConfig(item + "_nickname");
                if (id == "") id = item;
                materialComboBox_account_1.Items.Add(id);
                materialComboBox_account_2.Items.Add(id);
                materialComboBox_account_3.Items.Add(id);
            }

            byte current_preset = Byte.Parse(ConfigManager.getConfig("current_preset"));
            int[] index = Array.ConvertAll(ConfigManager.getConfig("current_comboBox_index_preset_" + current_preset).Split(';'), s => int.Parse(s));
            materialComboBox_account_1.SelectedIndex = index[0];
            materialComboBox_account_2.SelectedIndex = index[1];
            materialComboBox_account_3.SelectedIndex = index[2];
            materialComboBox_account_1.Refresh();
            materialComboBox_account_2.Refresh();
            materialComboBox_account_3.Refresh();
        }

        private void materialButton1_Click(object sender, EventArgs e) {
            MaterialForm form = new MaterialForm() {
                Size = new Size(1100, 800),
                FormStyle = FormStyles.ActionBar_None,
                StartPosition = FormStartPosition.CenterScreen
            };

            form.Controls.Add(webView_main);
            form.FormClosed += (sender, e) => {
                form.Controls.Clear();
            };
            
            form.Show();
        }

        private void LoadShortcut() {
            string[] names = ConfigManager.getConfig("shortcut_name").Split(';');
            materialButton_shortcut_1.Text = names[0];
            materialButton_shortcut_2.Text = names[1];
            materialButton_shortcut_3.Text = names[2];
            materialButton_shortcut_4.Text = names[3];
        }

        private void materialButton_naver_Click(object sender, EventArgs e) {
            MaterialButton searchButton = (MaterialButton)sender;
            MaterialSwitch? loginSwitch = null;

            if(searchButton.Equals(materialButton_search_1)) { loginSwitch = materialSwitch_login_1; } 
            else if(searchButton.Equals(materialButton_search_2)) { loginSwitch = materialSwitch_login_2; }
            else { loginSwitch = materialSwitch_login_3; }

            isSearch = true;
            if (true == loginSwitch.Checked) { webView_main.CoreWebView2.Navigate(url_search); } //네이버 검색
            else { SwitchClick(loginSwitch); }
        }

        private void radio_preset_CheckedChanged(object sender, EventArgs e) {
            MaterialRadioButton radio = (MaterialRadioButton)sender;
            string? value = radio.Tag.ToString();
            if(value == null) {
                MessageBox.Show("RadioButton의 Tag가 Null입니다.");
                return;
            }

            ConfigManager.setConfig("current_preset", value);
            LoadAccountComboBox();
        }

        private void LoadRadioButton() {
            byte current_preset = Byte.Parse(ConfigManager.getConfig("current_preset"));
            switch (current_preset) {
                case 1 :
                    radio_preset_1.PerformClick();
                    break;
                case 2 :
                    radio_preset_2.PerformClick();
                    break;
                case 3 :
                    radio_preset_3.PerformClick();
                    break;
                case 4 :
                    radio_preset_4.PerformClick();
                    break;
                default:
                    MessageBox.Show("LoadRadioButton에서 오류 발생");
                    break;
            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            Trace.WriteLine("Form Resize! : " + this.WindowState);
            if(this.WindowState == FormWindowState.Minimized) {
                //this.WindowState = FormWindowState.Normal;
            }
        }

        private async void Login(string id, string protected_pw) {
            try {
                await webView_main.ExecuteScriptAsync("document.getElementById('GSuserID').value = '" + id + "'");
                await webView_main.ExecuteScriptAsync("document.getElementById('GSuserPW').value = '" + EncryptionSupporter.Unprotect(protected_pw) + "'");
            } catch (CryptographicException e) {
                //사용자가 암호화된 패스워드가 포함된 설정파일을 타 PC로 복사 후 사용 시 발생하는 오류
                Trace.WriteLine(e.Message);
                MessageBox.Show("계정정보를 타 PC로 옮긴 것으로 확인되었습니다.\n계정 정보 유출 방지를 위해 모든 계정 정보를 초기화 합니다.", "패스워드 복호화 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                currentClient = Client.None;
                currentState = State.None;
                ClearAccount();
                return;
            }

            await webView_main.ExecuteScriptAsync("document.getElementById('btn_Login').click()");
            currentState = State.LoggedIn;
        }

        private void ClearAccount() {
            string temp = ConfigManager.getConfig("account_list");
            string[] account_list = temp.Remove(temp.Length - 1, 1).Split(';');
            foreach (var item in account_list) {
                ConfigManager.removeConfig(item);
            }
            ConfigManager.setConfig("account_list", "");

            ConfigManager.setConfig("current_comboBox_index_preset_1", "0;0;0");
            ConfigManager.setConfig("current_comboBox_index_preset_2", "0;0;0");
            ConfigManager.setConfig("current_comboBox_index_preset_3", "0;0;0");
            ConfigManager.setConfig("current_comboBox_index_preset_4", "0;0;0");
            LoadAccountComboBox();
        }

        private void materialButton_setting_Click(object sender, EventArgs e) {
            CustomButton button = (CustomButton)sender;
            if (button.Equals(materialButton_setting_account)) {
                OpenAccountSettingDialog();
            } else if (button.Equals(materialButton_setting_client)) {
                OpenClientSettingDialog();
            } else if (button.Equals(materialButton_setting_shortcut)) {
                OpenShortcuttSettingDialog();
            } else if (button.Equals(materialButton_setting_advanced)) {
                OpenAdvancedSettingDialog();
            }
        }

        private void OpenAdvancedSettingDialog() {
            Form backgroundForm = InitBackgroundForm(this);

            Form_AdvancedSetting dialog_shortcutSetting = new Form_AdvancedSetting() {
                Owner = this
            };

            try {
                backgroundForm.Show();
                dialog_shortcutSetting.ShowDialog();
            } catch (Exception ex) {
                Trace.WriteLine(ex.StackTrace);
            } finally {
                backgroundForm.Dispose();
            }
        }

        private void OpenShortcuttSettingDialog() {
            Form backgroundForm = InitBackgroundForm(this);

            Form_ShortcutSetting dialog_shortcutSetting = new Form_ShortcutSetting() {
                Owner = this
            };

            try {
                backgroundForm.Show();
                dialog_shortcutSetting.ShowDialog();
                LoadShortcut();
            } catch (Exception ex) {
                Trace.WriteLine(ex.StackTrace);
            } finally {
                backgroundForm.Dispose();
            }
        }

        private void OpenClientSettingDialog() {
            Form backgroundForm = InitBackgroundForm(this);

            Form_ClientSetting dialog_clientSetting = new Form_ClientSetting() {
                Owner = this
            };

            try {
                backgroundForm.Show();
                dialog_clientSetting.ShowDialog();
            } catch (Exception ex) {
                Trace.WriteLine(ex.StackTrace);
            } finally {
                backgroundForm.Dispose();
            }
        }

        private void OpenAccountSettingDialog() {
            Form backgroundForm = InitBackgroundForm(this);

            Form_AccountSetting dialog_accountSetting = new Form_AccountSetting() {
                Owner = this
            };

            try {
                backgroundForm.Show();
                dialog_accountSetting.ShowDialog();
                LoadAccountComboBox();
            } catch (Exception ex) {
                Trace.WriteLine(ex.StackTrace);
            } finally {
                backgroundForm.Dispose();
            }
        }

        private void materialCheckbox_testServer_CheckedChanged(object sender, EventArgs e) {
            ConfigManager.setConfig("is_test_server", ((MaterialCheckbox)sender).Checked.ToString());
        }

        public static Form InitBackgroundForm(Form owner) {
            Form backgroundForm = new Form() {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = .50d,
                BackColor = Color.Black,
                Location = owner.Location,
                Size = owner.Size,
                ShowInTaskbar = false,
                TopMost = true,
                Owner = owner
            };
            return backgroundForm;
        }

        private void materialButton_kakao_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo("https://open.kakao.com/o/sXJQ1qPd") { UseShellExecute = true });
        }

        private void materialButton_patchNote_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo(url_release) { UseShellExecute = true });
        }

        private void materialButton_blog_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo("https://blog.naver.com/kog5071/222644960946") { UseShellExecute = true });
        }

        private void materialButton_gitHub_Click(object sender, EventArgs e) {
            Process.Start(new ProcessStartInfo("https://github.com/byungmeo/GersangStation") { UseShellExecute = true });
        }

        private void button_tray_Click(object sender, EventArgs e) {
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipTitle = "알림";
            notifyIcon1.BalloonTipText = "프로그램이 트레이로 이동되었습니다.";
            notifyIcon1.ShowBalloonTip(5000);
            this.Hide();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e) {
            notifyIcon1.Visible = false;
            this.Show();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start(new ProcessStartInfo("https://logomakr.com/app") { UseShellExecute = true });
        }
    } //Form1
} //namespace
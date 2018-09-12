using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GrammarJudgment
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string grammar;
        List<string> VN;
        List<string> P;
        string type;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_judge_Click(object sender, RoutedEventArgs e)
        {
            if (judgeIllegal()) return;

            foreach(string production in P)
            {
                string left = production.Substring(0, production.IndexOf(":"));
                if (left.Length > 1)
                {
                    char[] charArr = left.ToCharArray();
                    foreach(char c in charArr)
                    {
                        if (isVN(left))
                        {

                        }
                    }
                }
            }


        }
        
        private bool judgeIllegal()
        {
            //判断是否有空值
            if (txt_G.Text.Trim().Length == 0 || txt_VN.Text.Trim().Length == 0 || lb_P.Items.Count == 0)
            {
                MessageBox.Show("输入不完整");
                return true;
            }

            //传入
            setData();

            //判断文法名
            Regex reg = new Regex(@"^G\[.\]$", RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(grammar);
            if (matches.Count == 0)
            {
                MessageBox.Show("文法名输入不正确");
                return true;
            }

            //判断非终结符
            List<string> tempList=new List<string>();
            foreach(string str in VN)
            {
                if (str.Trim().Length!=0)
                {
                    tempList.Add(str);
                }
            }
            VN = tempList;

            if (VN.Count == 0)
            {
                MessageBox.Show("非终结符不能为空");
                return true;
            }

            //判断产生式
            Regex reg2 = new Regex(@".*?::=.*?", RegexOptions.IgnoreCase);
            foreach(ListBoxItem lbi in lb_P.Items)
            {
                string str = lbi.Content.ToString();
                if (str.Trim().Length == 0)
                {
                    continue;
                }
                MatchCollection matches2 = reg2.Matches(str);
                if (matches2.Count != 1)
                {
                    MessageBox.Show("产生式输入不正确");
                    return true;
                }
            }

            return false;
        }

        private void setData()
        {
            grammar = txt_G.Text;

            txt_VN.Text = txt_VN.Text.Replace("，", ",");
            VN = Regex.Split(txt_VN.Text, ",", RegexOptions.IgnoreCase).ToList();

            P = new List<string>();
            foreach (ListBoxItem lbi in lb_P.Items)
            {
                P.Add(lbi.Content.ToString());
            }
        }

        private bool isVN(string s)
        {
            foreach(string str in VN)
            {
                if (str == s)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
}

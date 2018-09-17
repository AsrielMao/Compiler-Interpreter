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
        int type;
            public MainWindow()
            {
                InitializeComponent();
                txt_G.Text = "G[N]";
                txt_VN.Text = "N,D";
                lb_P.Items.Add("N::=ND|D");
                lb_P.Items.Add("D::=0|1|2|3|4|5|6|7|8|9");
            }

        private void btn_judge_Click(object sender, RoutedEventArgs e)
        {
            //判断输入是否合法
            if (judgeIllegal()) return;

            type = 3;
            //逐句判断文法类型
            foreach(string production in P)
            {
                int singleType = getSingleType(production);
                type = singleType < type ? singleType : type;
            }

            //清空右侧文本内容
            txt_Result.Clear();

            //输出
            txt_Result.Text = type + "型文法";

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
                if (str.Trim().Length!=1||includeOr(str))
                {
                    MessageBox.Show("请用单个字符作为非终结符,且不要用“|”作为非终结符！");
                    return true;
                }
                tempList.Add(str.Trim());
            }
            VN = tempList;

            if (VN.Count == 0)
            {
                MessageBox.Show("非终结符不能为空");
                return true;
            }

            //判断产生式
            Regex reg2 = new Regex(@".*?::=.*?", RegexOptions.IgnoreCase);
            foreach(string lbi in lb_P.Items)
            {
                string str = lbi;
                //判断是否这一项为空，空则跳过
                if (str.Trim().Length == 0)
                {
                    continue;
                }

                //判断是否有且只有一个“::=”符号
                MatchCollection matches2 = reg2.Matches(str);
                if (matches2.Count != 1)
                {
                    MessageBox.Show("产生式中“::=”数量不正确（中文赋值号不计数）");
                    return true;
                }

                //判断产生式左侧\右侧是否为空
                string left = lbi.Substring(0, lbi.IndexOf("::="));
                string right = lbi.Substring(lbi.IndexOf("::=") + 3);
                if (left.Trim().Length == 0 || right.Trim().Length == 0)
                {
                    MessageBox.Show("产生式左侧和右侧都不能为空！");
                    return true;
                }
                //判断左侧是否含有“|”符号
                if (includeOr(left))
                {
                    MessageBox.Show("产生式左侧不能含有“|”符号！");
                    return true;
                }
            }
            

            return false;
        }

        //获取一句产生式的类型
        private int getSingleType(string production)
        {
            string left = production.Substring(0, production.IndexOf("::="));
            string right = production.Substring(production.IndexOf("::=") + 3);
            
            int singleType = 3;

            if (left.Trim().Length > 1)
            {
                singleType = 0;
                char[] arr = left.ToCharArray();

                foreach(char c in arr)
                {
                    if (isVN(c.ToString()))
                    {
                        singleType = 1;
                        break;
                    }
                }
            }else if (!isVN(left))
            {
                singleType = 0;
            }
            else
            {
                List<string> rights = Regex.Split(right, "|", RegexOptions.IgnoreCase).ToList();
                singleType = 3;
                foreach(string r in rights)
                {
                    if (getRightType(r) == 2)
                    {
                        singleType = 2;
                        break;
                    }
                }

            }


            return singleType;
        }

        private int getRightType(string right)
        {
            if (right.Trim().Length == 1 && !isVN(right))
                return 3;
            if(right.Trim().Length==2
                    &&(
                    (isVN(right.Trim().Substring(0,1))&&!isVN(right.Trim().Substring(1, 1)))
                    || (isVN(right.Trim().Substring(1, 1)) && !isVN(right.Trim().Substring(0, 1)))
                    )
                )
            {
                return 3;
            }

            return 2;
        }

        private bool includeOr(string left)
        {
            char[] arr = left.ToCharArray();

            foreach(char c in arr)
            {
                if (c == '|')
                {
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
            foreach (string lbi in lb_P.Items)
            {
                P.Add(lbi);
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

        private void btn_up_Click(object sender, RoutedEventArgs e)
        {
            int count = lb_P.SelectedIndex;
            if (count < 0)
            {
                return;
            }
            string str = lb_P.SelectedItem.ToString();
            if (count > 0)
            {
                lb_P.Items[count] = lb_P.Items[count - 1];
                lb_P.Items[count - 1] = str;
                lb_P.SelectedIndex = count - 1;
            }
        }

        private void btn_down_Click(object sender, RoutedEventArgs e)
        {
            int count = lb_P.SelectedIndex;
            if (count < 0)
            {
                return;
            }
            string str = lb_P.SelectedItem.ToString();
            if (count < lb_P.Items.Count - 1)
            {
                lb_P.Items[count] = lb_P.Items[count + 1];
                lb_P.Items[count + 1] = str;
                lb_P.SelectedIndex = count + 1;
            }
        }
    }
    
}

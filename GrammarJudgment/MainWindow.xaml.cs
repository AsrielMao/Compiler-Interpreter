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
        string start;
        bool extendGrammar = false;
        List<string> VN;
        List<string> VT;
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

            //判断2、3型文法是否为扩充型
            extendGrammar = false;
            if (type > 1 && judgeExtend())
            {
                extendGrammar = true;
            }
            

            //输出
            printResult();

        }

        private void printResult()
        {
            //清空右侧文本内容
            txt_Result.Clear();

            //初始化
            string result = "文法 " + grammar + " =({";

            //非终结符
            foreach(string s in VN)
            {
                result += s + ", ";
            }
            result = result.Substring(0, result.Length - 2);

            result += "}, {";

            //非终结符
            foreach(string s in VT)
            {
                result += s + ", ";
            }
            result = result.Substring(0, result.Length - 2);

            result += "}, P, " + start + ")\nP: \n";

            //产生式
            foreach(string s in P)
            {
                result += "\t" + s + "\n";
            }

            //文法类型
            result += "\n该文法是" + (extendGrammar ? "扩充的" : "") + "Chomsky" + type + "型文法。";

            switch (type)
            {
                case 0:
                    result += "（即短语文法。）";
                    break;
                case 1:
                    result += "（即上下文有关文法。）";
                    break;
                case 2:
                    result += "（即上下文无关文法。）";
                    break;
                case 3:
                    result += "（即正规文法。）";
                    break;
            }

            txt_Result.Text = result;

        }
        
        //判断输入是否合法
        private bool judgeIllegal()
        {
            //判断是否有空值
            if (txt_G.Text.Trim().Length == 0 || txt_VN.Text.Trim().Length == 0 || lb_P.Items.Count == 0)
            {
                MessageBox.Show("输入不完整");
                return true;
            }

            //传入,去除所有空格
            setData();

            //判断文法名
            Regex reg = new Regex(@"^G\[.\]$", RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(grammar);
            if (matches.Count == 0)
            {
                MessageBox.Show("文法名输入不正确");
                return true;
            }

            //判断文法中是否确实含有起始符号
            if (judgeNoStart())
            {
                MessageBox.Show("非终结符或产生式左侧没有出现起始符号！");
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
            foreach(string lbi in P)
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

                //判断左侧是否不含非终结符
                bool noVN = true;
                foreach(char c in left.ToCharArray())
                {
                    if (isVN(c.ToString()))
                    {
                        noVN = false;
                        break;
                    }
                }
                if (noVN)
                {
                    MessageBox.Show("产生式左侧不能不含非终结符！");
                    return true;
                }

                //判断左侧是否含有“|”符号或者e
                if (includeOr(left)||includeE(left))
                {
                    MessageBox.Show("产生式左侧不能含有“|”符号和“e”符号！");
                    return true;
                }

                //判断右侧是否有空值（||）
                string tmpRight = right.Replace("，", ",");
                tmpRight = right.Replace("|", ",");
                string[] tmpArr = Regex.Split(tmpRight, ",", RegexOptions.IgnoreCase);
                foreach(string s in tmpArr)
                {
                    if (s.Length == 0)
                    {
                        MessageBox.Show("产生式右侧不能为空，空值请用e表示！也不要再产生式中出现逗号！");
                        return true;
                    }
                }
            }
            

            return false;
        }

        //判断2、3型文法是否为扩充型
        private bool judgeExtend()
        {
            foreach(string str in P)
            {
                string right = str.Substring(str.IndexOf("::=") + 3);
                if (includeE(right))
                {
                    return true;
                }
            }

            return false;
        }

        //判断非终结符和产生式左侧是否含有起始符号，若不含有，返回true
        private bool judgeNoStart()
        {
            if (isVN(start))
            {
                foreach (string p in P)
                {
                    char[] arr = p.Substring(0, p.IndexOf("::=")).ToCharArray();

                    foreach (char c in arr)
                    {
                        if (c.ToString() == start)
                        {
                            return false;
                        }
                    }
                }
            }
            

            return true;
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
                        string tmpRight = right.Replace("|", ",");
                        foreach (string str in Regex.Split(tmpRight, ",", RegexOptions.IgnoreCase))
                        {
                            singleType = 0;
                            if (left.Length>str.Length)
                            {
                                break;
                            }
                            singleType = 1;
                        }
                        
                        break;
                    }
                }
            }else if (!isVN(left))
            {
                singleType = 0;
            }
            else
            {
                string tmpRight = right.Replace("|", ",");
                List<string> rights = Regex.Split(tmpRight, ",", RegexOptions.IgnoreCase).ToList();
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

        //左侧已经确定只含有一个非终结符的情况下，判断右侧
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

        //判断是否含有e
        private bool includeE(string str)
        {
            foreach(char c in str.ToCharArray())
            {
                if (c.ToString() == "e")
                {
                    return true;
                }
            }

            return false;
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
            txt_G.Text = txt_G.Text.ToString().Replace(" ", "");
            grammar = txt_G.Text;

            start = grammar.Substring(2, 1);

            txt_VN.Text = txt_VN.Text.Replace("，", ",");
            txt_VN.Text = txt_VN.Text.Replace(" ", "");

            VN = Regex.Split(txt_VN.Text, ",", RegexOptions.IgnoreCase).ToList();

            P = new List<string>();
            for(int i=0;i<lb_P.Items.Count;i++)
            {

                lb_P.Items[i] = lb_P.Items[i].ToString().Replace(" ", "");
                P.Add(lb_P.Items[i].ToString());
            }

            VT = new List<string>();

            foreach(string str in P)
            {
                string s = str.Replace("::=", "");
                s = s.Replace("|", "");
                char[] chars = s.ToCharArray();

                foreach(char c in chars)
                {
                    if (isVT(c.ToString())||isVN(c.ToString()))
                    {
                        continue;
                    }
                    VT.Add(c.ToString());
                }
            }
        }

        private bool isVT(string str)
        {
            foreach(string s in VT)
            {
                if (s == str)
                {
                    return true;
                }
            }

            return false;
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

        private void cbx_example_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbx_example.SelectedIndex)
            {
                case 0:
                    txt_G.Text = "G[N]";
                    txt_VN.Text = "N,D";
                    lb_P.Items.Clear();
                    lb_P.Items.Add("N::=ND|D");
                    lb_P.Items.Add("D21::=ND|D");
                    lb_P.Items.Add("D::=0|1|2|3|4|5|6|7|8|9");
                    break;
                case 1:
                    txt_G.Text = "G[N]";
                    txt_VN.Text = "N,D";
                    lb_P.Items.Clear();
                    lb_P.Items.Add("N::=ND|D");
                    lb_P.Items.Add("N0::=ND|D");
                    lb_P.Items.Add("D::=0|1|2|3|4|5|6|7|8|9");
                    break;
                case 2:
                    txt_G.Text = "G[N]";
                    txt_VN.Text = "N,D";
                    lb_P.Items.Clear();
                    lb_P.Items.Add("N::=ND|D");
                    lb_P.Items.Add("D::=0|1|2|3|4|5|6|7|8|9");
                    break;
                case 3:
                    txt_G.Text = "G[N]";
                    txt_VN.Text = "N,D";
                    lb_P.Items.Clear();
                    lb_P.Items.Add("N::=D0|0");
                    lb_P.Items.Add("D::=1|2|3|4|5|6|7|8|9");
                    break;
            }
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            int count = lb_P.SelectedIndex;
            if (count < 0)
            {
                return;
            }
            lb_P.Items.RemoveAt(count);
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            PEditor pEditor = new PEditor();
            pEditor.txt_P.Text = "123";
            if(pEditor.ShowDialog() == true)
            {
                string result = pEditor.getResult();

                if (result.Trim().Length <= 0)
                {
                    return;
                }

                lb_P.Items.Add(result);
            }

        }

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {
            int idx = lb_P.SelectedIndex;
            if (idx < 0)
            {
                return;
            }

            PEditor pEditor = new PEditor();
            pEditor.txt_P.Text = lb_P.SelectedItem.ToString();

            if (pEditor.ShowDialog() == true)
            {
                string result = pEditor.getResult();

                if (result.Trim().Length <= 0)
                {
                    return;
                }

                lb_P.Items[idx] = result;
            }
        }
    }

    
}

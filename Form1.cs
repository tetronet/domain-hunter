using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ShortDomainHunter
{
    public partial class Form1 : Form
    {
        private ConcurrentDictionary<string, IPAddress?> Found = [];
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = false;
            numericUpDown2.Enabled = false;
            checkBox1.Enabled = false;
            button1.Enabled = false;
            progressBar1.Maximum = (int)numericUpDown2.Value;
            Task.Run(CheckDomains);
        }
        private void CheckDomains()
        {
            for (int i = 1; i < numericUpDown2.Value; i++)
            {
                try
                {
                    IPAddress addr = Dns.GetHostAddresses(ToLetters(i) + textBox1.Text)[0];
                    Found.TryAdd(ToLetters(i) + textBox1.Text, addr);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.NoData || ex.SocketErrorCode == SocketError.HostNotFound)
                    {
                        Found.TryAdd(ToLetters(i) + textBox1.Text, null);
                    }
                    else
                    {
                        Found.TryAdd(ToLetters(i) + textBox1.Text, IPAddress.Any);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    Found.TryAdd(ToLetters(i) + textBox1.Text, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while scrolling domains: " + ex, "Tsu-k Interruptive Window", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                try
                {
                    progressBar1.Invoke(delegate ()
                    {
                        progressBar1.Increment(1);
                    });
                }
                catch { }
            }
            listBox1.Invoke(delegate ()
            {
                foreach (KeyValuePair<string, IPAddress?> kv in Found)
                {
                    if (kv.Value == null)
                    {
                        listBox1.Items.Add($"[FREE] Domain={kv.Key}");
                    }
                    else
                    {
                        if (checkBox1.Checked)
                        {
                            listBox1.Items.Add($"[CLAIMED] Domain={kv.Key} Owner={kv.Value.MapToIPv4()}");
                        }
                    }
                }
            });
        }
        private static string ToLetters(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be bigger than 0");
            }
            StringBuilder sb = new();
            while (value > 0)
            {
                value--;
                sb.Insert(0, (char)('a' + value % 26));
                value /= 26;
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Drawing;
using System.Windows.Forms;

namespace MasterP
{
    class Program : Form
    {
        public Program()
        {
            var t = new TextBox();

            t.Parent = this;
        }

        static void Main(string[] args)
        {
            Application.Run(new Program());

            Console.Write("word #1: ");
            var w1 = Console.ReadLine();

            Console.Write("word #2: ");
            var w2 = Console.ReadLine();

            Console.Write("from: ");
            int from = Int32.Parse(Console.ReadLine());

            Console.Write("to: ");
            int to = Int32.Parse(Console.ReadLine());

            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.ASCII.GetBytes(w1 + '-' + w2 + '\n');
                byte[] hash_code = md5.ComputeHash(bytes);
                byte[] hex_code = new byte[hash_code.Length * 2];

                for(int i = 0; i < hash_code.Length; i++)
                {
                    byte b = hash_code[i];
                    var hb = (b >> 4);
                    var lb = (b & 0xF);

                    hex_code[i * 2    ] = (byte)(hb < 10 ? '0' + hb : 'a' + hb - 10);
                    hex_code[i * 2 + 1] = (byte)(lb < 10 ? '0' + lb : 'a' + lb - 10);
                }

                //string x = BitConverter.ToString(hash_code).ToLower().Replace("-", "");
                //byte[] x_bytes = System.Text.Encoding.UTF8.GetBytes(x);
                var s = Convert.ToBase64String(hex_code);

                Console.WriteLine(s.Substring(from - 1, to - from + 1));
            }

            Console.ReadLine();

        }
    }
}

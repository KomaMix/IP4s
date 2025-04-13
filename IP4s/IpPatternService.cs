using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP4s
{
    public class IpPatternService
    {
        public List<string> GetIpAddressesByPattern(string mathPattern)
        {
            List<string> candidates = new List<string>();

            // Ограничиваем диапазон для первых двух байт (0..15) для демонстрации
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int leftSum = i + j;
                    for (int k = 0; k < 256; k++)
                    {
                        for (int l = 0; l < 256; l++)
                        {
                            int rightSum = k + l;
                            if (EvaluateCondition(leftSum, rightSum, mathPattern))
                            {
                                string ip = $"{i}.{j}.{k}.{l}";
                                candidates.Add(ip);
                            }
                        }
                    }
                }
            }
            return candidates;
        }

        private bool EvaluateCondition(int left, int right, string op)
        {
            switch (op)
            {
                case "==": return left == right;
                case "!=": return left != right;
                case "<": return left < right;
                case ">": return left > right;
                case "<=": return left <= right;
                case ">=": return left >= right;
                default: return left == right;
            }
        }
    }
}

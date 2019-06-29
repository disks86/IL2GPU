using System;
using System.Collections.Generic;
using System.Text;

namespace IL2GPU_Compiler
{
    public static class Utilities
    {
        /// <summary>
        /// Combined the word count and opcode into a single word.
        /// </summary>
        /// <param name="wordCount"></param>
        /// <param name="opcode"></param>
        /// <returns></returns>
        public static UInt32 Pack(Int16 wordCount, SpirVOpCode opcode)
        {
            var opcodeDescription = new PackStructure();

            opcodeDescription.WordCount = wordCount;
            opcodeDescription.Opcode = opcode;

            return opcodeDescription.Word;
        }

        /// <summary>
        /// Pack 4 characters into a single word.
        /// </summary>
        /// <param name="char1"></param>
        /// <param name="char2"></param>
        /// <param name="char3"></param>
        /// <param name="char4"></param>
        /// <returns></returns>
        public static UInt32 Pack(char char1, char char2, char char3,char char4)
        {
            var opcodeDescription = new PackStructure();

            opcodeDescription.Char1 = char1;
            opcodeDescription.Char2 = char2;
            opcodeDescription.Char3 = char3;
            opcodeDescription.Char4 = char4;

            return opcodeDescription.Word;
        }

        /// <summary>
        /// Pack a string into words (including null terminator) and add them to a list of words.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="words"></param>
        public static void PutStringInList(string text, List<UInt32> words)
        {
            char[] value = text.ToCharArray();
            for (Int32 i = 0; i < (Int32)text.Length; i += 4)
            {
                Int32 difference = text.Length - (i);

                switch (difference)
                {
                    case 0:
                        break;
                    case 1:
                        words.Add(Pack('\0', '\0', '\0', value[i]));
                        break;
                    case 2:
                        words.Add(Pack('\0', '\0', value[i + 1], value[i]));
                        break;
                    case 3:
                        words.Add(Pack('\0', value[i + 2], value[i + 1], value[i]));
                        break;
                    default:
                        words.Add(Pack(value[i + 3], value[i + 2], value[i + 1], value[i]));
                        break;
                }
            }

            if (text.Length % 4 == 0)
            {
                words.Add(0); //null terminator if all words have characters.
            }
        }
    }
}

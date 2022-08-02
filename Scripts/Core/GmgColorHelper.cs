using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GestureManager.Scripts.Core
{
    public static class GmgColorHelper
    {
        static readonly byte[] _uHexT =
        {
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0x0A,0x0B,0x0C,0x0D,0x0E,0x0F,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF
        };
        
        static bool IsValidHtmlColor(string hexcode)
        {
            if (hexcode.Length != 7 && hexcode.Length != 9)
            {
                return false;
            }

            if (hexcode[0] != '#')
            {
                return false;
            }

            for (int i = 1; i < hexcode.Length; i++)
            {
                if (_uHexT[hexcode[i]] == 0xFF)
                {
                    return false;
                }
            }

            return true;
        }
        
        public static bool TryParseHtmlColor(string hexcode, out Color32 color)
        {
            if (IsValidHtmlColor(hexcode))
            {
                switch (hexcode.Length)
                {
                    case 7:
                        color = new Color32(
                            (byte)((_uHexT[hexcode[1]] << 4) | _uHexT[hexcode[2]]),
                            (byte)((_uHexT[hexcode[3]] << 4) | _uHexT[hexcode[4]]),
                            (byte)((_uHexT[hexcode[5]] << 4) | _uHexT[hexcode[6]]),
                            255);
                        return true;
                    case 9:
                        color = new Color32(
                            (byte)((_uHexT[hexcode[1]] << 4) | _uHexT[hexcode[2]]),
                            (byte)((_uHexT[hexcode[3]] << 4) | _uHexT[hexcode[4]]),
                            (byte)((_uHexT[hexcode[5]] << 4) | _uHexT[hexcode[6]]),
                            (byte)((_uHexT[hexcode[7]] << 4) | _uHexT[hexcode[8]])
                        );
                        return true;
                    default:
                        break;
                }
            }

            color = Color.clear;
            return false;
        }
        public static bool TryParseTMPColor(string color, out Color32 result)
        {
            if (!TryParseHtmlColor(color, out result))
            {
                switch (color)
                {
                    case "red": result = Color.red; break;
                    case "green": result = Color.green; break;
                    case "blue": result = Color.blue; break;
                    case "white": result = Color.white; break;
                    case "black": result = Color.black; break;
                    case "yellow": result = new Color(1f, 0.92f, 0f); break;
                    case "orange": result = new Color(1f, 0.5f, 0f); break;
                    case "purple": result = new Color(0.63f, 0.13f, 0.94f); break;
                    // case "cyan": result = Color.cyan; break; // Not supported by TextMeshPro
                    // case "magenta": result = Color.magenta; break; // Not supported by TextMeshPro
                    // case "gray:
                    // case "grey": result = new Color(0.5f, 0.5f, 0.5f); break; // Not supported by TextMeshPro
                    // case "clear": result = Color.clear; break; // Not supported by TextMeshPro
                    default: return false;
                }
            }

            return true;
        }
    }
}
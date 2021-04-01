using System;
using System.Collections.Generic;
using Thundershock;

namespace RedTeam.SaveData
{
    public static class StaticGameDataRegistry
    {
        public static IEnumerable<HackableType> GetPossibleHackableTypes(DeviceType device)
        {
            switch (device)
            {
                case DeviceType.Atm:

                    break;
                case DeviceType.Database:
                    yield return HackableType.Database;
                    break;
                case DeviceType.Player:
                    yield return HackableType.Shell;
                    yield return HackableType.RemoteDesktop;
                    yield return HackableType.Vpn;
                    break;
                case DeviceType.Pos:
                    
                    break;
                case DeviceType.Router:
                    
                    break;
                case DeviceType.Website:
                    yield return HackableType.Shell;
                    yield return HackableType.FileTransfer;
                    yield return HackableType.Web;
                    yield return HackableType.WebSecure;
                    break;
                case DeviceType.Workstation:
                    yield return HackableType.Shell;
                    break;
                case DeviceType.ChatServer:
                    yield return HackableType.Database;
                    yield return HackableType.Chat;
                    break;
                case DeviceType.FileServer:
                    yield return HackableType.FileTransfer;
                    yield return HackableType.Shell;
                    break;
                case DeviceType.GameServer:
                    yield return HackableType.GameServer;
                    yield return HackableType.Database;
                    break;
                case DeviceType.MailServer:
                    yield return HackableType.Mail;
                    yield return HackableType.Database;
                    break;
            }
        }

        public static string GetGenericHackableName(HackableType type)
        {
            return type switch
            {
                HackableType.Chat => "Chat Server",
                HackableType.Shell => "Secure Shell",
                HackableType.FileTransfer => "FTP Server",
                HackableType.Mail => "IMAP Mail Server",
                HackableType.Database => "SQL Database",
                HackableType.Web => "Web Server (HTTP)",
                HackableType.WebSecure => "Web Server (HTTPS)",
                HackableType.Vpn => "Virtual Private Network",
                HackableType.RemoteDesktop => "Remote Desktop",
                HackableType.GameServer => "Game Server",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static int GetDefaultHackablePort(HackableType hackableType)
        {
            return hackableType switch
            {
                HackableType.Shell => 22,
                HackableType.FileTransfer => 21,
                HackableType.Mail => 587,
                HackableType.Database => 3899,
                HackableType.Web => 80,
                HackableType.WebSecure => 443,
                HackableType.Vpn => 420,
                HackableType.RemoteDesktop => 3900,
                HackableType.Chat => 6667,
                HackableType.GameServer => 867,
                _ => throw new ArgumentOutOfRangeException(nameof(hackableType), hackableType, null)
            };
        }

        public static string GetGenericHackableDescription(HackableType hackableType)
        {
            return hackableType switch
            {
                HackableType.Shell =>
                    "Secure shell server for controlling a device remotely over a terminal connection.",
                HackableType.FileTransfer => "Outdated file transfer service.",
                HackableType.Mail => "For sending, receiving and storing email messages.",
                HackableType.Database => "Table-based data storage server.",
                HackableType.Web => "Website with no encryption.",
                HackableType.WebSecure => "Website with encryption.",
                HackableType.Vpn => "Virtual private network server",
                HackableType.RemoteDesktop => "For controlling a device remotely through a GUI",
                HackableType.Chat => "Instant messaging service",
                HackableType.GameServer => "Online multi-player and leaderboards for a game.",
                _ => throw new ArgumentOutOfRangeException(nameof(hackableType), hackableType, null)
            };
        }

        public static string GetHackableId(HackableType type)
        {
            return type switch
            {
                HackableType.Shell => "ssh",
                HackableType.FileTransfer => "ftp",
                HackableType.Mail => "imap",
                HackableType.Database => "sql",
                HackableType.Web => "http",
                HackableType.WebSecure => "https",
                HackableType.Vpn => "vpn",
                HackableType.RemoteDesktop => "vnc",
                HackableType.Chat => "irc",
                HackableType.GameServer => "mmo",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static IEnumerable<HackableType> GetNetworkHackables(NetworkType netType)
        {
            switch (netType)
            {
                case NetworkType.Home:
                    yield return HackableType.Shell;
                    break;
                case NetworkType.School:
                    yield return HackableType.Web;
                    yield return HackableType.WebSecure;
                    yield return HackableType.Mail;
                    break;
                case NetworkType.Corporate:
                    yield return HackableType.Web;
                    yield return HackableType.WebSecure;
                    yield return HackableType.Mail;
                    yield return HackableType.Database;
                    break;
                case NetworkType.StoreFront:
                    yield return HackableType.Shell;
                    yield return HackableType.Database;
                    yield return HackableType.Web;
                    yield return HackableType.WebSecure;
                    break;
                case NetworkType.Financial:
                    break;
                case NetworkType.Government:
                    break;
                case NetworkType.RedTeamAgency:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(netType), netType, null);
            }
        }
    }
}
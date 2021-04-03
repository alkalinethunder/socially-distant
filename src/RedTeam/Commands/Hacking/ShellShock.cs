using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RedTeam.HackPayloads;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam.Commands.Hacking
{
    public sealed class ShellShock : HackCommand
    {
        private SoundEffect _heartbeat;
        private double _heartbeatScanTimeLeft = 0;
        private double _interval;
        private int _state = 0;
        private Random _rnd = new Random();
        private List<string> _keyLines = new List<string>();
        
        public override string Name => "shellshock";

        protected override void StartHack()
        {
            RejectAnythingBut(HackableType.Shell);

            _heartbeat = EntryPoint.CurrentApp.Content.Load<SoundEffect>("Sounds/Hacking/Heartbleed");
            
            Console.WriteLine(" * ssh connected *");

            _interval = 0.25;
            _heartbeatScanTimeLeft = Hops * 0.25;
        }

        protected override void OnHackUpdate(float deltaTime)
        {
            if (_state == 0)
            {
                if (_interval < 0)
                {
                    _heartbeat.Play();
                    _interval = 1;
                    Console.WriteLine(" * sending 64k HEARTBEAT cmd *");
                }

                _interval -= deltaTime;

                _heartbeatScanTimeLeft -= deltaTime;
                if (_heartbeatScanTimeLeft <= 0)
                {
                    Console.WriteLine("-----&bFOUND SSH PRIVATE KEY&B-----");
                    for (var i = 0; i < 25; i++)
                    {
                        var line = "";
                        for (var j = 0; j < 50; j++)
                        {
                            line += (char) _rnd.Next((int) '0', (int) 'Z');
                        }

                        _keyLines.Add(line);
                    }
                    
                    _state++;
                    _interval = 0.05;
                }
            }
            else if (_state == 1)
            {
                if (_interval < 0)
                {
                    var line = _keyLines.First();
                    Console.WriteLine(line);
                    _keyLines.RemoveAt(0);
                    if (_keyLines.Count <= 0)
                    {
                        _state++;
                        Console.WriteLine("-----&b    ACCESS GRANTED   &B-----");
                        Console.WriteLine();
                        Console.WriteLine(" * deploying redrev to target... *");
                        Console.WriteLine();
                        _interval = 2;
                    }
                    else
                    {
                        _interval = 0.05;
                    }
                }

                _interval -= deltaTime;
            }
            else if (_state == 2)
            {
                _interval -= deltaTime;
                if (_interval < 0)
                {
                    Console.WriteLine(" * done *");
                    CompleteHack<ReverseShell>();
                }
            }
        }
    }
}
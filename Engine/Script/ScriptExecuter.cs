﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Engine.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Script
{
    public static class ScriptExecuter
    {
        private static readonly Dictionary<string, int> Variables = new Dictionary<string, int>();
        private static float _fadeTransparence;

        public static float FadeTransparence
        {
            get { return _fadeTransparence; }
            private set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                _fadeTransparence = value;
            }
        }

        public static bool IsInFadeOut = false;
        public static bool IsInFadeIn = false;

        public static void Update(GameTime gameTime)
        {
            if (IsInFadeOut && FadeTransparence < 1f)
            {
                FadeTransparence += 0.02f;
            }
            else if (IsInFadeIn && FadeTransparence > 0f)
            {
                FadeTransparence -= 0.02f;
                if (FadeTransparence <= 0f) IsInFadeIn = false;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            
        }

        public static void Say(List<string> parameters)
        {
            switch (parameters.Count)
            {
                case 1:
                    GuiManager.ShowDialog(Utils.RemoveStringQuotes(
                        parameters[0]));
                    break;
                case 2:
                    GuiManager.ShowDialog(Utils.RemoveStringQuotes(
                        parameters[0]),
                        int.Parse(parameters[1]));
                    break;
            }
        }

        private static readonly Regex IfParameterPatten = new Regex(@"(\$[a-zA-Z]+) *([><=]+) *(.+)");
        public static bool If(List<string> parameters)
        {
            var parmeter = parameters[0];
            var match = IfParameterPatten.Match(parmeter);
            if (match.Success)
            {
                var groups = match.Groups;
                var variable = groups[1].Value;
                var compare = groups[2].Value;
                var value = int.Parse(groups[3].Value);
                if (Variables.ContainsKey(variable))
                {
                    switch (compare)
                    {
                        case "==":
                            return Variables[variable] == value;
                        case ">>":
                            return Variables[variable] > value;
                        case ">=":
                            return Variables[variable] >= value;
                        case "<<":
                            return Variables[variable] < value;
                        case "<=":
                            return Variables[variable] <= value;
                        case "<>":
                            return Variables[variable] != value;
                    }
                    
                }
            }
            return false;
        }

        public static void Add(List<string> parameters)
        {
            var variable = parameters[0];
            var value = int.Parse(parameters[1]);
            if (!Variables.ContainsKey(variable))
                Variables[variable] = 0;
            Variables[variable] += value;
        }

        public static void Assign(List<string> parameters)
        {
            var variable = parameters[0];
            var value = int.Parse(parameters[1]);
            Variables[variable] = value;
        }

        public static void FadeOut()
        {
            IsInFadeOut = true;
            IsInFadeIn = false;
            FadeTransparence = 0f;
        }

        public static bool IsFadeOutEnd()
        {
            return FadeTransparence >= 1f;
        }

        public static void FadeIn()
        {
            IsInFadeOut = false;
            IsInFadeIn = true;
            FadeTransparence = 1f;
        }

        public static bool IsFadeInEnd()
        {
            return !IsInFadeIn;
        }

        public static void DrawFade(SpriteBatch spriteBatch)
        {
            var fadeTextrue = TextureGenerator.GetColorTexture(
                    Color.Black * ScriptExecuter.FadeTransparence,
                    1,
                    1);
            spriteBatch.Draw(fadeTextrue,
                new Rectangle(0, 0, Globals.WindowWidth, Globals.WindowHeight),
                Color.White);
        }

        public static void DeleteNpc(List<string> parameters)
        {
            NpcManager.DeleteNpc(Utils.RemoveStringQuotes(parameters[0]));
        }

        public static void ClearBody()
        {
            ObjManager.ClearBody();
        }

        public static void StopMusic()
        {
            BackgroundMusic.Stop();
        }

        public static void PlayMusic(List<string> parameters)
        {
            BackgroundMusic.Play(Utils.RemoveStringQuotes(parameters[0]));
        }


        public static void PlaySound(List<string> parameters, object belongObject)
        {
            var fileName = Utils.RemoveStringQuotes(parameters[0]);
            var sound = Utils.GetSoundEffect(fileName);
            var soundPosition = Globals.ListenerPosition;
            var sprit = belongObject as Sprite;
            if (sprit != null) soundPosition = sprit.PositionInWorld;
            
            SoundManager.Play3DSoundOnece(sound, soundPosition - Globals.ListenerPosition);
        }

        public static void OpenBox(List<string> parameters, object belongObject)
        {
            if (parameters == null)
            {
                var obj = belongObject as Obj;
                if (obj != null)
                {
                    obj.OpenBox();
                }
            }
            else
            {
                var obj = ObjManager.GetObj(Utils.RemoveStringQuotes(parameters[0]));
                if (obj != null)
                {
                    obj.OpenBox();
                }
            }
        }

        public static void SetObjScript(List<string> parameters, object belongObject)
        {
            var objName = Utils.RemoveStringQuotes(parameters[0]);
            var scriptFileName = Utils.RemoveStringQuotes(parameters[1]);
            var obj = belongObject as Obj;
            ScriptParser script = null;
            if (!string.IsNullOrEmpty(objName))
                obj = ObjManager.GetObj(objName);
            if(!string.IsNullOrEmpty(scriptFileName))
                script = new ScriptParser(Utils.GetScriptFilePath(scriptFileName), obj);
            if (obj != null)
            {
                obj.ScriptFile = script;
            }
        }

        public static void AddRandMoney(List<string> parameters)
        {
            var min = int.Parse(parameters[0]);
            var max = int.Parse(parameters[1]) + 1;
            var money = Globals.TheRandom.Next(min, max);
            Globals.ThePlayer.AddMoney(money);
        }
    }
    // List<string> parameters
}
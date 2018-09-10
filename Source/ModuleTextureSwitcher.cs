using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using FinePrint;
using Upgradeables;
using KSP.UI.Screens;

/*
Source code copyrighgt 2017, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
If you want to use this code, give me a shout on the KSP forums! :)
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace Switchers
{
    public struct TextureSwitchOption
    {
        public string diffuseMap;
        public string normalMap;
        public string displayName;
    }

    public class ModuleTextureSwitcher : BaseSwitcher
    {
        [KSPField()]
        public string transformName = string.Empty;

        public Dictionary<string, TextureSwitchOption> textureOptions = new Dictionary<string, TextureSwitchOption>();

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }

        public override void ToggleOption()
        {
            base.ToggleOption();

            SetTexture();
        }

        public override void ToggleOption(int optionIndex)
        {
            base.ToggleOption(optionIndex);

            SetTexture();
        }

        public override ConfigNode[] GetOptionNodes(string nodeName = kOptionNode)
        {
            ConfigNode[] optionNodes = base.GetOptionNodes(nodeName);
            TextureSwitchOption option;

            textureOptions.Clear();
            foreach (ConfigNode node in optionNodes)
            {
                option = new TextureSwitchOption();

                option.displayName = node.GetValue("name");

                if (node.HasValue("diffuseMap") == false)
                    continue;
                option.diffuseMap = node.GetValue("diffuseMap");

                if (node.HasValue("normalMap"))
                    option.normalMap = node.GetValue("normalMap");

                textureOptions.Add(option.displayName, option);
            }

            return optionNodes;
        }

        public void SetTexture()
        {
            if (string.IsNullOrEmpty(transformName))
                return;
            TextureSwitchOption textureOption = textureOptions[optionNames[currentOptionIndex]];
            Transform[] targets;
            Texture2D texture;
            Renderer rendererMaterial;

            if (string.IsNullOrEmpty(textureOption.diffuseMap))
                return;

            //Get the targets
            targets = part.FindModelTransforms(transformName);
            if (targets == null)
            {
                Debug.Log("No targets found for " + transformName);
            }

            //Now, replace the textures in each target
            foreach (Transform target in targets)
            {
                rendererMaterial = target.GetComponent<Renderer>();

                texture = GameDatabase.Instance.GetTexture(textureOption.diffuseMap, false);
                if (texture != null)
                    rendererMaterial.material.SetTexture("_MainTex", texture);

                if (!string.IsNullOrEmpty(textureOption.normalMap))
                {
                    texture = GameDatabase.Instance.GetTexture(textureOption.normalMap, false);
                    if (texture != null)
                        rendererMaterial.material.SetTexture("_BumpMap", texture);
                }
            }
        }
    }
}

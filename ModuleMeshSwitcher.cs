using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code by Michael Billard (Angel-125)
License: GPLV3

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace Switchers
{
    public struct MeshOption
    {
        public string name;
        public string meshTransforms;
    }

    public class ModuleMeshSwitcher : BaseSwitcher, IMultipleDragCube
    {
        protected DictionaryValueList<string, MeshOption> meshOptions = new DictionaryValueList<string, MeshOption>();
        protected string[] meshTransforms = null;

        [KSPField]
        public bool synchronizeWithResourceSwitcher;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //Watch for resource switch events
            if (synchronizeWithResourceSwitcher)
            {
                ModuleResourceSwitcher resourceSwitcher = this.part.FindModuleImplementing<ModuleResourceSwitcher>();
                if (resourceSwitcher != null)
                    resourceSwitcher.onSwitchedToOption += onSwitchedToOption;
            }

            //Set up the meshes for the current option.
            SetVisibleMeshes();
        }

        public override void ToggleOption()
        {
            base.ToggleOption();

            //Set up the meshes for the current option
            SetVisibleMeshes();
        }

        public override void ToggleOption(int optionIndex)
        {
            base.ToggleOption(optionIndex);

            //Set up the meshes for the current option
            SetVisibleMeshes();
        }

        protected void onSwitchedToOption(string name)
        {
            //Do we have a mesh option with the same name?
            for (int index = 0; index < optionNames.Length; index++)
            {
                if (optionNames[index] == name)
                {
                    currentOptionIndex = index;
                    SetVisibleMeshes();
                    return;
                }
            }
        }

        public virtual void SetVisibleMeshes()
        {
            if (meshTransforms.Length == 0)
                return;
            string optionName = optionNames[currentOptionIndex];
            MeshOption meshOption = meshOptions[optionName];

            Events["ToggleOption"].guiName = optionNames[currentOptionIndex];

            for (int index = 0; index < meshTransforms.Length; index++)
            {
                if (meshOption.meshTransforms.Contains(meshTransforms[index]))
                    setMeshVisible(meshTransforms[index], true);
                else
                    setMeshVisible(meshTransforms[index], false);
            }
        }

        protected virtual void setMeshVisible(string meshName, bool isVisible)
        {
            Transform[] targets;

            //Get the targets
            targets = part.FindModelTransforms(meshName);
            if (targets == null)
            {
                Debug.Log("No targets found for " + meshName);
                return;
            }

            foreach (Transform target in targets)
            {
                //Show/hide mesh
                target.gameObject.SetActive(isVisible);

                //Show/hide collider
                Collider collider = target.gameObject.GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = isVisible;
            }
        }

        public override ConfigNode[] GetOptionNodes(string nodeName = kOptionNode)
        {
            ConfigNode[] nodes = base.GetOptionNodes(nodeName);
            if (nodes == null)
                return null;

            ConfigNode node = null;
            MeshOption meshOption;
            string[] transformNames;
            List<string> meshNames = new List<string>();

            //Now process all the nodes
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];

                if (node.HasValue("meshTransforms") && node.HasValue("name"))
                {
                    //Create a MeshOption
                    meshOption = new MeshOption();
                    meshOption.name = node.GetValue("name");
                    meshOption.meshTransforms = node.GetValue("meshTransforms");

                    //If we've got transforms to process then add the option to our database.
                    transformNames = meshOption.meshTransforms.Split(';');
                    if (transformNames.Length > 0)
                    {
                        //Add the option
                        meshOptions.Add(meshOption.name, meshOption);
 
                        //Add all the mesh transforms
                        for (int meshIndex = 0; meshIndex < transformNames.Length; meshIndex++)
                        {
                            if (string.IsNullOrEmpty(transformNames[meshIndex]) == false)
                                meshNames.Add(transformNames[meshIndex]);
                        }
                    }
                }
            }

            //If we've got options, then generate the array of meshes.
            if (meshNames.Count > 0)
                meshTransforms = meshNames.ToArray();

            return nodes;
        }

        #region IMultipleDragCube
        public void AssumeDragCubePosition(string name)
        {
        }

        public string[] GetDragCubeNames()
        {
            return null;
        }

        public bool IsMultipleCubesActive
        {
            get
            {
                return true;
            }
        }

        public bool UsesProceduralDragCubes()
        {
            return true;
        }
        #endregion
    }
}

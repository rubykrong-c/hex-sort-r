using Code.Core.Slots;
using UnityEngine;
using Zenject;
using Code;
using Code.Core.Slots.Deck;

namespace Code.Core.Settings
{
    [CreateAssetMenu(fileName = "CoreSettingsInstaller", menuName = "Installers/CoreSettingsInstaller")]
    public class CoreSettingsInstaller: ScriptableObjectInstaller<CoreSettingsInstaller>
    {
        public HexDeckHandler.HexColorMaterialDictionary ColorCodeMaterialsDict => _levelLoaderSettings.ColorMaterials;
#pragma warning disable 0649
        [SerializeField]
        private HexDeckHandler.Settings _levelLoaderSettings;
#pragma warning restore 0649

        public override void InstallBindings()
        {
            Container.BindInstance(_levelLoaderSettings).WhenInjectedInto<HexDeckHandler>();
        }
        
        
    }
}
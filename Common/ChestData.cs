using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;

namespace TerrariaInGameWorldEditor.Common
{
    public class ChestData
    {
        public string Name { get; set; } = "";
        public List<(int type, int stack, int prefix)> Items { get; set; } = new List<(int type, int stackSize, int prefix)>();

        public ChestData()
        {

        }

        public void Place(int x, int y)
        {  
            int index = Chest.CreateChest(x, y);
            if (index == -1)
            {
                return;
            }
            Chest chest = Main.chest[index];
            chest.name = Name;

            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].type == ItemID.None)
                {
                    continue;
                }
                chest.item[i] = new Item();
                chest.item[i].SetDefaults(Items[i].type);
                chest.item[i].stack = Items[i].stack;
                chest.item[i].Prefix(Items[i].prefix);
            }
        }

        public static ChestData CopyChestData(int x, int y)
        {
            if (TileID.Sets.IsAContainer[Main.tile[x, y].TileType] || TileID.Sets.BasicChest[Main.tile[x, y].TileType])
            {
                int index = Chest.FindChest(x, y);
                if (index != -1)
                {           
                    ChestData cd = new ChestData();
                    Chest chest = Main.chest[index];
                    cd.Name = chest.name;
                    foreach (Item item in chest.item)
                    {
                        cd.Items.Add((item.type, item.stack, item.prefix));
                    }
                    return cd;
                }
            }
            return null;
        }

        public static void Write(BinaryWriter bw, ChestData cd)
        {
            bw.Write(cd != null);
            if (cd != null)
            {
                bw.Write(cd.Name);
                bw.Write(cd.Items.Count);
                foreach (var item in cd.Items)
                {
                    bw.Write(ItemID.Search.GetName(item.type));
                    bw.Write(item.stack);
                    bw.Write(item.prefix);
                }
            }
        }

        public static ChestData Read(BinaryReader br, HashSet<string> missingMods)
        {
            bool hasChestData = br.ReadBoolean();
            if (!hasChestData)
            {
                return null;
            }

            ChestData cd = new ChestData();
            cd.Name = br.ReadString();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int type;
                string name = br.ReadString();
                if (ItemID.Search.TryGetId(name, out int id))
                {
                    type = (ushort)id;
                }
                else
                {
                    missingMods.Add(name.Split('/')[0]);
                    type = ItemID.None;
                }
                int stackSize = br.ReadInt32();
                int prefix = br.ReadInt32();
                cd.Items.Add((type, stackSize, prefix));
            }
            return cd;
        }
    }
}

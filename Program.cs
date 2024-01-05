using System;
using System.Numerics;
using static TextRPG.Program;

namespace TextRPG
{
    internal class Program
    {
        public interface ICharacter //캐릭터, 몬스터 등 공용
        {
            string Name { get; set; }
            int Attack { get; set; }
            int Defense { get; set; }
            int Health { get; set; }
        }

        public class Warrior : ICharacter
        {
            public int Level { get; set; }
            public string Name { get; set; }
            public string Job { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int Health { get; set; }
            public int Gold { get; set; }
            private Item AtkWeapon { get; set; } //무기
            private Item DefArmor { get; set; } //갑옷

            public List<Item> inven = new List<Item>();

            //전사, 용병, 마법사 스탯
            string[] jobs = { "전사", "용병", "마법사" };
            int[] attacks = { 10, 8, 12 };
            int[] defenses = { 7, 10, 5 };
            int[] healthes = { 100, 80, 50 };
            int[] golds = { 1500, 1200, 2000 };

            public Warrior(string name, int job)
            {
                Level = 1;
                Name = name;
                Job = jobs[job - 1];
                Attack = attacks[job - 1];
                Defense = defenses[job - 1];
                Health = healthes[job - 1];
                Gold = golds[job - 1];
            }

            public void ShowStatus()
            {
                TextColor("\n상태 보기", ConsoleColor.Yellow);
                Console.WriteLine("캐릭터의 정보가 표시됩니다.\n");
                Console.WriteLine("    Lv. " + Level.ToString("D2"));
                Console.WriteLine("    " + Name + " ( " + Job + " )");
                Console.WriteLine("    공격력 : " + Attack);
                Console.WriteLine("    방어력 : " + Defense);
                Console.WriteLine("    체  력 : " + Health);
                Console.WriteLine("    골  드 : {0} G", Gold);
            }

            public void EquipItem(int num)
            {
                Item item = inven[num];
                //item을 장착하면 스탯 변화
            }

            public int ShowInven(bool isManaging)
            {
                Console.WriteLine("[아이템 목록]\n");

                if (inven.Count <= 0)
                {
                    Console.WriteLine("\n인벤토리에 아이템이 없습니다.\n상점에서 아이템을 구매해주세요.\n");
                    int cmd = CheckAction("0. 나가기", 0, 0);

                    if(cmd == 0)
                    {
                        Village(this);
                    }
                }

                int num = 1;
                foreach (Item item in inven)
                {
                    string equipTxt = "- ";

                    if(isManaging)
                    {
                        equipTxt += num.ToString() + " ";
                        if (item.isEquipped)
                        {
                            equipTxt += "[E]";
                        }
                    }

                    equipTxt += ItemList(item);
                    Console.WriteLine(equipTxt);
                    num++;
                }

                return num;
            }

            public bool isOwned(Item item)
            {
                foreach(Item ownedItem in inven)
                {
                    if(ownedItem.ItemNum == item.ItemNum)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void BuyItem(Item item)
            {
                Console.WriteLine();
                if(!isOwned(item))
                {
                    if (item.ItemGold <= Gold)
                    {
                        Gold -= item.ItemGold;
                        inven.Add(item);
                        Console.WriteLine("'{0}'을 구매했습니다.", item.ItemName);
                    }
                    else
                    {
                        TextColor("Gold가 부족합니다.", ConsoleColor.Red);
                    }
                } else
                {
                    Console.WriteLine("이미 구매한 아이템입니다.");
                }
            }
        }

        static string ItemList(Item item)
        {
            string atkTxt = "";
            string defTxt = "";

            if (item.ItemAtk != 0)
            {
                atkTxt = "| 공격력 +" + item.ItemAtk + " |";
            }
            if (item.ItemDef != 0)
            {
                defTxt = "| 방어력 +" + item.ItemAtk + " |";
            }

            string summary = item.ItemName.PadRight(8) + "   " + atkTxt + defTxt + item.ItemSummary;

            return summary;
        }

        public class Item
        {
            public int ItemNum { get; }
            public string ItemName { get; }
            public int ItemPart { get; }
            public int ItemAtk { get; }
            public int ItemDef { get; }
            public int ItemGold { get; }
            public bool isEquipped { get; set; }
            public string ItemSummary { get; }

            public Item(int itemNum, string itemName, int itemPart, int itemAtk, int itemDef, string itemSummary, int itemGold)
            {
                ItemNum = itemNum;
                ItemName = itemName;
                ItemPart = itemPart; //0은 갑옷 1은 무기
                ItemAtk = itemAtk;
                ItemDef = itemDef;
                ItemGold = itemGold;
                isEquipped = false;
                ItemSummary = itemSummary;
            }

        }

        static void Village(Warrior character)
        {
            TextColor("\n[광장]\n", ConsoleColor.Yellow);
            Console.WriteLine("     1. 상태 보기");
            Console.WriteLine("     2. 인벤토리");
            Console.WriteLine("     3. 상점\n");

            int cmd = CheckAction("어떤 활동을 하시겠습니까?", 1, 3);

            switch (cmd)
            {
                case 1:
                    //상태보기
                    character.ShowStatus();
                    cmd = CheckAction("\n0. 나가기\n\n원하시는 행동을 입력해주세요.", 0, 0);
                    if(cmd == 0)
                    {
                        Village(character);
                    }
                    break;
                case 2:
                    //인벤토리
                    ManageInven(character);
                    break;
                case 3:
                    //상점
                    Store(character);
                    break;
            }
        }
        static void ManageInven(Warrior character)
        {
            TextColor("\n인벤토리", ConsoleColor.Yellow);
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

            character.ShowInven(false);

            Console.WriteLine("\n1. 장착 관리");
            Console.WriteLine("0. 나가기\n");
            int cmd = CheckAction("원하는 행동을 입력해주세요.\n", 0, 1);

            if(cmd == 0)
            {
                //나가기
                Village(character);
            } else
            {
                // 장착 관리
                int max = character.ShowInven(true);
                int itemNum = CheckAction("\n장착/장착 해제할 아이템을 선택해주세요.", 1, max);
                //아이템 장착 character.EquipItem() 아마 번호 파라미터로넣을듯?
            }
        }

        static void Store(Warrior character)
        {
            Item[] lv1 = { new Item(0, "수련자 갑옷", 0, 0, 5, " 초보자를 위한 수련에 도움을 주는 갑옷.", 500),
                           new Item(1, "낡은 검", 1, 2, 0, " 쉽게 부서질 것 같은 낡은 검.", 300),
                           new Item(2, "청동 도끼", 1, 4, 0, " 방금까지도 나무를 벤 것 같은 흔한 도끼.", 900)};

            Item[] lv2 = { new Item(3,"무쇠 갑옷", 0, 0, 10, " 숙련자가 만든 튼튼한 갑옷.", 1200),
                           new Item(4,"가벼운 은검", 1, 7, 0, " 휴대하기 좋은 날카로운 은검.", 1200)};

            Item[] lv3 = { new Item(5,"장인의 갑옷", 0, 0, 15, " 장인이 만든 갑옷. 1년에 단 1개만 제작된다고 한다.", 2000),
                           new Item(6,"장인의 검", 1, 10, 0, " 장인이 만든 검. 스치기만 해도 치명상을 입는다.", 1600),
                           new Item(7,"장인의 활", 1, 7, 3, " 장인이 만든 활. 먼 거리에서도 화살이 올곧게 날아간다.", 1600)};

            //전설의 전용 무기
            Item[] lv5 = { new Item(8,"이카본의 쌍검", 2, 20, 10, " 초대 군주 이카본이 기사단장에게 수여했다고 전해지는 전설의 검. 기사만이 사용할 수 있다.", 5000),
                           new Item(9,"용병왕의 대검", 3, 15, 15, " 사라진 용병왕의 대검. 용병이 아닌 자는 들 수 없다.", 5000),
                           new Item(10,"아나로즈의 망토", 4, 10, 10, " 수천년을 산 마녀 아나로즈의 망토. 마법사가 아닌 자는 그 용도를 알 수 없다.", 5000)};

            List<Item> shopItem = new List<Item>();
            if(character.Level == 1)
            {
                for(int i = 0; i < lv1.Length; i++)
                {
                    shopItem.Add(lv1[i]);
                }
            } else if(character.Level == 2)
            {
                for (int i = 0; i < lv2.Length; i++)
                {
                    shopItem.Add(lv2[i]);
                }
            } else if( character.Level == 3)
            {
                for (int i = 0; i < lv3.Length; i++)
                {
                    shopItem.Add(lv3[i]);
                }
            } else if(character.Level == 5)
            {
                for (int i = 0; i < lv5.Length; i++)
                {
                    shopItem.Add(lv5[i]);
                }
            }

            TextColor("\n[상점]", ConsoleColor.Yellow);
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.");

            Console.WriteLine("\n[보유 골드]");
            TextColor(character.Gold + " G\n", ConsoleColor.Yellow);

            int cnt = 1;
            foreach (Item item in shopItem)
            {
                string equipTxt = "- " + cnt + " ";

                equipTxt += ItemList(item);
                //구매한 아이템이라면 구매완료로 분리
                if(character.isOwned(item))
                {
                    equipTxt += " | 구매 완료";
                } else
                {
                    equipTxt += " | " + item.ItemGold.ToString() + " G";
                }
                
                TextColor(equipTxt, ConsoleColor.Gray);
                cnt++;
            }

            Console.WriteLine("\n1. 아이템 구매");
            Console.WriteLine("0. 나가기\n");

            int cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 1);
            if (cmd == 0)
            {
                Village(character);
            }
            else
            {
                int itemNum = CheckAction("\n구매할 아이템 번호를 선택해주세요.", 1, shopItem.Count);
                character.BuyItem(shopItem[itemNum - 1]);
            }

            Console.WriteLine("\n1. 아이템 구매 계속하기");
            Console.WriteLine("0. 나가기\n");
            cmd = CheckAction("원하는 행동을 입력해주세요.", 0, 1);

            if (cmd == 0)
            {
                Village(character);
            } else
            {
                Store(character);
            }
        }

        static int CheckAction(string start, int min, int max)
        {
            while(true)
            {
                Console.WriteLine(start);
                Console.Write(">> ");

                int ans;
                if(int.TryParse(Console.ReadLine(), out ans))
                {
                    if(ans >= min && ans <= max)
                    {
                        return ans;
                    }
                }
                Console.WriteLine("\n잘못된 입력입니다. 다시 선택해주십시오.");
            }
        }

        static void TextColor(string text, ConsoleColor clr)
        {
            Console.ForegroundColor = clr;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static void Main(string[] args)
        {
            Warrior character;
            string name;

            //게임 시작 화면
            Console.WriteLine("\n\n~~***~~TEXT RPG 모험의 시작~~***~~\n\n");

            while(true)
            {
                Console.WriteLine("당신의 이름은 무엇입니까?");
                name = Console.ReadLine();
                Console.WriteLine("\n '{0}' 이 당신의 이름이 맞습니까?\n", name);
                Console.WriteLine("     1. 맞습니다.");
                Console.WriteLine("     2. 아닙니다.\n");
                Console.Write(">> ");
                string ans = Console.ReadLine();

                if (ans == "1")
                {
                    Console.WriteLine("{0}님 환영합니다.\n", name);
                    break;
                }
                else
                {
                    Console.WriteLine("이름은 바꿀 수 없으니 신중히 입력하십시오.\n");
                    continue;
                }
            }
            Console.WriteLine("                     직업을 선택해주세요.\n");

            Console.WriteLine("         |   1. 전사    |   2. 용병    |  3. 마법사  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 공격력  |      10      |      8       |      12  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 방어력  |      7       |      10      |      5  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 체  력  |     100      |      80      |      50  ");
            Console.WriteLine("---------|--------------|--------------|--------------");
            Console.WriteLine(" 골  드  |     1500     |     1200     |     2000  ");

            int job = CheckAction("", 1, 3);
            character = new Warrior(name, job);

            Console.WriteLine("\n{0} {1}님, 스파르타 마을에 오신걸 환영합니다.", character.Job, character.Name);
            Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.");

            Village(character);
        }
    }
}

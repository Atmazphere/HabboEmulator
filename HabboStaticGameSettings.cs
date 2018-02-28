using Emulator.Database.Interfaces;
using Emulator.HabboHotel.Users;
using System;

namespace Emulator
{
         class HabboStaticGameSettings
            {
                /// <summary>
                ///     enable/disable the targetted offers on login.
                /// </summary>
                public const bool TARGETED_OFFER_ENABLED = true;

                /// <summary>
                ///     Double Currency.
                /// </summary>
                public const bool DoubleCurrency = false;
                /// <summary>
                ///     The amount of credits a user will recieve every x minutes
                /// </summary>
                public const int UserCreditsUpdateAmount = 50;

                /// <summary>
                ///     XMAS calandar enabled??
                /// </summary>
                public const bool CampaignCalendarEnabled = false;
    
                /// <summary>
                ///     The amount of pixels a user will recieve every x minutes
                /// </summary>
                public const int UserPixelsUpdateAmount = 100;
    
                /// <summary>
                ///     The Room Id for the Advertiser Ban room.
                /// </summary>
                public const int AdvRoomBanId = 83232323;

                /// <summary>
                ///     Default max number of visitors for when creating a room.
                /// </summary>
                public const int MaxRoomVisitorsDefault = 100;
                /// <summary>
                ///     The amount of Diamonds a user will recieve every x minutes
                /// </summary>
                ///              
                public const int UserDiamondsUpdateAmount = 1;

                /// <summary>
                ///    The Required rank to add a room to the staff picks.
                /// </summary>
                public const int CanStaffPickRank = 3;

                /// <summary>
                ///     The Default amount of items that can be selected via trigger wired.
                /// </summary>
                public const int DefaultTriggerWiredSelect = 15;
        
                /// <summary>
                ///     The Default amount of items that can be selected via Condition wired.
                /// </summary>
                public const int DefaultConditionWiredSelect = 15;

                /// <summary>
                ///     The Default amount of items that can be selected via Effect wired.
                /// </summary>
                public const int DefaultEffectWiredSelect = 15;

                /// <summary>
                ///     The time a user will have to wait for Credits/Pixels update in minutes
                /// </summary>
                public const int UserCreditsUpdateTimer = 15;
               
                /// <summary>
                ///     The time it will check if a user has earnt the next level of HC.
                /// </summary>
                public const int HCUpdateTimer = 30;

                /// <summary>
                ///     The time a user will have to wait for Diamonds update in minutes
                /// </summary>
                public const int UserDiamondsUpdateTimer = 30;

                /// <summary>
                ///     The time a user will have to wait for Diamonds update in minutes
                /// </summary>
                public const int UserClubUpdateTimer = 30;
                /// <summary>
                ///     The maximum amount of furniture that can be placed in a room.
                /// </summary>
                public const int RoomFurnitureLimit = 20000;

                /// <summary>
                ///     The maximum amount of pet instances that can be placed into a room.
                /// </summary>
                public const int RoomPetPlacementLimit = 100;

                /// <summary>
                ///     The maximum amount of pet instances that can be placed into a room.
                /// </summary>
                public const int RoomBotPlaceLimit = 50;

                /// <summary>
                ///     The amount of pets a user can place in a room that isn't owned by them.
                /// </summary>
                public const int UserPetPlacementRoomLimit = 50;

                /// <summary>
                ///     The maximum life time of a room promotion.
                /// </summary>
                public const int RoomPromotionLifeTime = 120;

                /// <summary>
                ///     The maximum amount of friends a user can have.
                /// </summary>
                public const int MessengerFriendLimit = 5000;

                /// <summary>
                ///     The amount of credits a group costs.
                /// </summary>
                public const int GroupPurchaseAmount = 150;

                /// <summary>
                ///     The maximum amount of banned words that a client can send per session.
                /// </summary>
                public const int BannedPhrasesAmount = 6;

                /// <summary>
                /// The maximum amount of members a group can exceed before being eligible for deletion.
                /// </summary>
                public const int GroupMemberDeletionLimit = 500;
                
                /// <summary>
                /// NUX instructions to send the user.
                /// </summary>
                public static string[] NuxInstructions = new string[] {     "helpBubble/add/BOTTOM_BAR_INVENTORY/nux.bot.info.inventory.1",
                                                                            "helpBubble/add/BOTTOM_BAR_NAVIGATOR/nux.bot.info.navigator.1",
                                                                            "helpBubble/add/chat_input/nux.bot.info.chat.1",
                                                                            "nux/lobbyoffer/show" };
            }
        }

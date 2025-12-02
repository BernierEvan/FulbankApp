using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FulbankApp.Helpers
{
    /// <summary>
    /// Constantes globales de l'application Fulbank
    /// </summary>
    public static class Constants
    {
        #region Animation Timings
        public const int FADE_IN_DURATION_MS = 400;
        public const int FADE_OUT_DURATION_MS = 300;
        public const int ZOOM_ANIMATION_DURATION_MS = 500;
        public const int BUTTON_PRESS_DURATION_MS = 100;
        public const int HALO_ANIMATION_DURATION_MS = 200;
        public const int SKIN_PANEL_ANIMATION_DURATION_MS = 300;
        public const int CHARACTER_WALK_DURATION_MS = 800;
        #endregion

        #region Zoom & Scale Values
        public const double ZOOM_HOVER_SCALE = 1.1;
        public const double ZOOM_CLICK_SCALE = 1.2;
        public const double BUTTON_PRESS_SCALE = 0.9;
        public const double SKIN_PANEL_INITIAL_SCALE = 0.8;
        public const double HALO_PULSE_MIN_SCALE = 1.0;
        public const double HALO_PULSE_MAX_SCALE = 1.2;
        public const double CHARACTER_BOUNCE_SCALE = 1.15;
        #endregion

        #region Opacity Values
        public const double DARK_OVERLAY_OPACITY = 0.6;
        public const double HALO_VISIBLE_OPACITY = 0.3;
        public const double HALO_HIDDEN_OPACITY = 0.0;
        public const double BUTTON_HIDDEN_OPACITY = 0.0;
        public const double BUTTON_VISIBLE_OPACITY = 1.0;
        #endregion

        #region Gameplay
        public const double PLAYER_SPEED = 3.5;
        public const double CHARACTER_SPEED_PPS = 175.0; // Pixels per second
        public const int MOVEMENT_TIMER_INTERVAL_MS = 20;
        public const int ANIMATION_FRAME_INTERVAL_MS = 16; // ~60 FPS
        public const int PIN_CODE_LENGTH = 8;
        #endregion

        #region Canvas Dimensions
        public const double CANVAS_WIDTH = 1920;
        public const double CANVAS_HEIGHT = 1080;
        #endregion

        #region Character Dimensions
        public const double CHARACTER_WIDTH = 200;
        public const double CHARACTER_HEIGHT = 200;
        public const double CHARACTER_RENDER_WIDTH = 450;
        public const double CHARACTER_RENDER_HEIGHT = 450;
        #endregion

        #region File Paths
        public const string CHARACTERS_BASE_PATH = "C:\\Users\\BERNIER\\source\\repos\\MaximeHenault\\FULBANK\\Fulbank\\assets\\characters";
        public const string DEFAULT_MALE_SKIN = "default_male";
        public const string DEFAULT_FEMALE_SKIN = "default_female";
        public const string THREE_PIECE_MAN_SKIN = "three_piece_man";
        public const string SAD_EMPLOYEE_SKIN = "sad_employee";
        public const string WEIRD_TURTLE_SKIN = "weird_turtle";

        #endregion

        #region Resource Paths (Pack URIs)
        public const string PACK_URI_BASE = "pack://application:,,,/Fulbank;component";

        public static string GetCharacterWalkGifPath(string skinName, string direction) =>
            $"{PACK_URI_BASE}/assets/characters/{skinName}/animations/walk/gifs/walk_{direction}.gif";

        public static string GetCharacterIdlePath(string skinName, string direction) =>
            $"{PACK_URI_BASE}/assets/characters/{skinName}/idle/idle_{direction}.png";
        #endregion

        #region Collision Detection
        public const double COLLISION_TOLERANCE = 0.0001;
        public const double HITBOX_SHRINK = 0.0;
        #endregion

        #region Mask Update Throttling
        public const int MASK_UPDATE_THROTTLE_MS = 80;
        public const double MASK_POSITION_THRESHOLD = 0.5;
        public const double MASK_ROTATION_THRESHOLD = 0.25;
        #endregion

        #region CURRENT USER

        public const string CURRENT_USER_NAME = "";
        public const string CURRENT_USER_SURNAME = "";
        public static readonly DateOnly CURRENT_USER_BIRTHDATE;
        public const string CURRENT_USER_EMAIL = "";
        public const string CURRENT_USER_PASSWORD = "";
        public static Image CURRENT_USER_PROFILEPICTURE = null;

        public static readonly List<string> BeneficiariesList = new List<string>();
        public static readonly List<string> PreviousTransfers = new List<string>();
        public static readonly List<string> PreviousConversions = new List<string>();

        public const decimal BankAmount = 0;
        public static readonly Dictionary<string, decimal> CryptosAmount = new Dictionary<string, decimal>();

        static Constants()
        {
            CURRENT_USER_BIRTHDATE = DateOnly.FromDateTime(DateTime.Today);
        }

        #endregion

        #region SETTINGS

        public const bool SMS_NOTIFICATION = false;
        public const bool EMAIL_NOTIFICATION = false;
        public const bool CONNECTION_ALERT = false;
        public const bool SECURITY_2FA = false;

        #endregion
    }
}
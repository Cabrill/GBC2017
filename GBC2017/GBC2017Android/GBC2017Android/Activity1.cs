#if ANDROID

using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using GBC2017.AndroidSpecific.GooglePlay.Services.Helpers;
using HockeyApp.Android;
using HockeyApp.Android.Metrics;
using Android.Content;
using Android.Widget;
using Android.Gms.Common;

namespace GBC2017
{
    [Activity(Label = "GBC2017"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.Multiple
        , ScreenOrientation = ScreenOrientation.UserLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        static SystemUiFlags f =
            SystemUiFlags.LayoutStable
            | SystemUiFlags.LayoutFullscreen
            | SystemUiFlags.Fullscreen;

        SignInButton signInButton;
        Button signOutButton;
        LinearLayout signInLayout;
        LinearLayout controlsLayout;
        Button awardAchievementButton;
        EditText achievementCode;
        Button showAchievements;
        Button showAllLeaderboards;
        Button showLeaderboard;
        Button submitScore;
        EditText leaderboardCode;
        EditText score;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RenderOnUIThread = true;

            MetricsManager.Register(Application, GetString(Resource.String.hockey_app_id));

            RequestWindowFeature(WindowFeatures.NoTitle);

            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
            {
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)f;
                ActionBar?.Hide();
            }

            SetContentView(Resource.Layout.Main);
            InitializeGooglePlayServices();
            

            AssignGooglePlayReferences();

            var g = new Game1();

            // FRB needs access to the activity to load fonts from the content:
            g.Services.AddService<Activity>(this);

            var gameView = (OpenTK.Platform.Android.AndroidGameView)g.Services.GetService(typeof(View));

            gameView.GenericMotion += HandleGenericMotion;
            //view.KeyPress += HandleKeyPress;
            FindViewById<RelativeLayout>(Resource.Id.container).AddView(gameView, 0);
            g.Run();

#if !DEBUG
            CheckForUpdates();
        }

        private void CheckForUpdates()
        {
            // Remove this for store builds!
            UpdateManager.Register(this, GetString(Resource.String.hockey_app_id));
        }

        private void UnregisterManagers()
        {
            UpdateManager.Unregister();
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterManagers();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterManagers();
        }
#else
        }
#endif

        private void InitializeGooglePlayServices()
        {
            GooglePlayServices.Instance.Initialize(this, viewforPopups: FindViewById<View>(Resource.Id.container));

            var helper = GooglePlayServices.Instance;
            helper.OnSignedIn += (object sender, EventArgs e) => {
                signInLayout.Visibility = ViewStates.Gone;
                controlsLayout.Visibility = ViewStates.Visible;
            };
            helper.OnSignInFailed += (object sender, EventArgs e) => {
                signInLayout.Visibility = ViewStates.Visible;
                controlsLayout.Visibility = ViewStates.Gone;
            };
        }

        private void AssignGooglePlayReferences()
        {
            var helper = GooglePlayServices.Instance;
            signInLayout = FindViewById<LinearLayout>(Resource.Id.signinlayout);
            controlsLayout = FindViewById<LinearLayout>(Resource.Id.controlslayout);

            signInButton = FindViewById<SignInButton>(Resource.Id.signin);
            signInButton.Click += (sender, args) =>
            {
                if (helper != null && helper.SignedOut)
                {
                    helper.SignIn();
                }
            };

            signOutButton = FindViewById<Button>(Resource.Id.signout);
            signOutButton.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    helper.SignOut();
                    signInLayout.Visibility = ViewStates.Visible;
                    controlsLayout.Visibility = ViewStates.Gone;
                }
            };

            awardAchievementButton = FindViewById<Button>(Resource.Id.awardachievement);
            achievementCode = FindViewById<EditText>(Resource.Id.achievementcode);

            awardAchievementButton.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    helper.UnlockAchievement(achievementCode.Text);
                }
            };

            showAchievements = FindViewById<Button>(Resource.Id.showachievement);
            showAchievements.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    helper.ShowAchievements();
                }
            };

            leaderboardCode = FindViewById<EditText>(Resource.Id.leaderboardcode);
            score = FindViewById<EditText>(Resource.Id.score);

            showAllLeaderboards = FindViewById<Button>(Resource.Id.showallleaderboards);
            showAllLeaderboards.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    helper.ShowAllLeaderBoardsIntent();
                }
            };

            showLeaderboard = FindViewById<Button>(Resource.Id.showleaderboard);
            showLeaderboard.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    var code = leaderboardCode.Text;
                    helper.ShowLeaderBoardIntentForLeaderboard(code);
                }
            };

            submitScore = FindViewById<Button>(Resource.Id.submitscore);
            submitScore.Click += (sender, args) =>
            {
                if (helper != null && !helper.SignedOut)
                {
                    var code = leaderboardCode.Text;
                    var value = int.Parse(score.Text);
                    helper.SubmitScore(code, value);
                }
            };
        }


        protected override void OnStart()
        {
            base.OnStart();
            GooglePlayServices.Instance?.Start();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            GooglePlayServices.Instance?.OnActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnStop()
        {
            GooglePlayServices.Instance?.Stop();
            base.OnStop();
        }

        protected override void OnResume()
        {
            base.OnResume();
            CrashManager.Register(this, GetString(Resource.String.hockey_app_id));

            if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
            {
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)f;
                ActionBar?.Hide();
            }
        }

        private void HandleGenericMotion(object sender, View.GenericMotionEventArgs e)
        {
            if ((e.Event.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad ||
                (e.Event.Source & InputSourceType.Joystick) == InputSourceType.Joystick)
            {
                FlatRedBallAndroid.Input.AndroidGamePadManager.OnGenericMotionEvent(e.Event);
            }
        }

        private void HandleKeyPress(object sender, View.KeyEventArgs e)
        {
            if ((e.Event.Source & InputSourceType.Gamepad) == InputSourceType.Gamepad)
            {
                if (e.Event.Action == KeyEventActions.Down)
                {
                    FlatRedBallAndroid.Input.AndroidGamePadManager.OnKeyDown(e.KeyCode, e.Event);
                }
                if (e.Event.Action == KeyEventActions.Up)
                {
                    FlatRedBallAndroid.Input.AndroidGamePadManager.OnKeyUp(e.KeyCode, e.Event);
                }
            }
        }

    }
}


#endif

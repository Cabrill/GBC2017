#if ANDROID

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace GBC2017
{
    [Activity(Label = "GBC2017"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.UserLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
    public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new Game1();

            // FRB needs access to the activity to load fonts from the content:
            g.Services.AddService<Activity>(this);


            var view = (OpenTK.Platform.Android.AndroidGameView)g.Services.GetService(typeof(View));

            view.KeyPress += HandleKeyPress;
            view.GenericMotion += HandleGenericMotion;
            //view.KeyPress += HandleKeyPress;
            SetContentView(view);
            g.Run();
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

using Foundation;
using System;
using UIKit;
using CoreGraphics;
using CandyCaneClub.Api.Models;

namespace CandyCaneClub.iOS.Extensions
{
    public class KeyboardAccessoryView : UIView
    {
        public UIButton[] buttons;
        public UITextView entryTextField;
        nfloat parentHeight, parentWidth;
        public static KeyboardAccessoryView GetView(UIView parentView, int intNumberOfButtons)
        {
            return new KeyboardAccessoryView(parentView, intNumberOfButtons);
        }

        private KeyboardAccessoryView(UIView parentView, int intNumberOfButtons)
        {
            //Frame initialise
            this.Frame = new CGRect(0, parentView.Bounds.Height - 100, parentView.Bounds.Width, 50);
            buttons = new UIButton[intNumberOfButtons];
            parentHeight = parentView.Bounds.Height;
            parentWidth = parentView.Bounds.Width;

            //Initialize textfield
            if(intNumberOfButtons == 1)
            {
                entryTextField = new UITextView(new CGRect(10, 10, ((parentView.Bounds.Width * 85) / 100), 30)); // 70% of screen area will be given to textfield
            }
            else
            {
                entryTextField = new UITextView(new CGRect(10, 10, ((parentView.Bounds.Width * 70) / 100), 30)); // 70% of screen area will be given to textfield
            }
            
            entryTextField.BackgroundColor = UIColor.White;

            entryTextField.Delegate = new AccessoryViewTextFieldDelegate();
            //Initialize all buttons
            initAllButtons(buttons, parentView.Bounds);

            //Register for keyboard notification
            registerNotification(parentView.Bounds);

            //Add keyboard accessory view to parent view
            parentView.AddSubview(this);

            //Add other subviews to accessory view
            this.AddSubview(entryTextField);
            foreach (UIButton button in buttons)
                this.AddSubview(button);

            //Set the extravalues to accessoryview
            this.BackgroundColor = UIColor.Red;
        }
        /*
        Function : Initialize / allocate memory for required amount of buttons
        */
        void initAllButtons(UIButton[] buttons, CGRect bounds)
        {
            nfloat initialOffset;
            if (buttons.Length == 1)
            {
                initialOffset = ((bounds.Width * 85) / 100);
                nfloat xPosition = (initialOffset + ((bounds.Width * ((0 + 1) * 3)) / 100)); //Calculate next x position by adding 3% of offset plus other component
                buttons[0] = new UIButton(new CGRect(xPosition, 10, ((bounds.Width * 10) / 100), 30)); //10% of the area will be occupied by button (max 3 buttons)
            }
            else
            {
                initialOffset = ((bounds.Width * 70) / 100);
                for (int index = 0; index < buttons.Length; index++)
                {
                    nfloat xPosition = (initialOffset + ((bounds.Width * ((index + (1 + (index*3))) * 3)) / 100)); //Calculate next x position by adding 5% of offset plus other component
                    buttons[index] = new UIButton(new CGRect(xPosition, 10, ((bounds.Width * 10) / 100), 30)); //10% of the area will be occupied by button (max 3 buttons)
                }
            }
            
        }
        /*
         Function: Registers current class for receiving notification from keyboard
        */
        void registerNotification(CGRect bounds)
        {
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, keyboardWillShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, keyboardDidShow);
            NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, keyboardWillHide);
        }
        void keyboardDidShow(NSNotification notification)
        {
            

        }
        void keyboardWillShow(NSNotification notification)
        {
            //TODO: find a scenario for this
			CGRect bounds = UIKeyboard.BoundsFromNotification(notification);
			moveView(bounds, notification);
        }
        void keyboardWillHide(NSNotification notification)
        {
            CGRect bounds = UIKeyboard.BoundsFromNotification(notification);
            moveView(bounds, notification);
        }
        /*
        Function: Generic function to move the view along with the keyboard 
        */
        void moveView(CGRect bounds, NSNotification notification)
        {
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.3);
            if(ChitChatViewController.CurrentHeightForAccessory != null && ChitChatViewController.CurrentHeightForAccessory > 0)
            {
                //This block is executed when we call the accessory view from ChitChatViewController. i.e. Multiline textView
                if (notification.Name.Equals("UIKeyboardWillShowNotification"))
                    this.Frame = new CGRect(0, ((parentHeight - ChitChatViewController.CurrentHeightForAccessory) - bounds.Height), parentWidth, ChitChatViewController.CurrentHeightForAccessory);
                else
                    this.Frame = new CGRect(0, parentHeight - (ChitChatViewController.CurrentHeightForAccessory + 50), parentWidth, ChitChatViewController.CurrentHeightForAccessory);
            }
            else
            {
                //This can be called from anywhere
                if (notification.Name.Equals("UIKeyboardWillShowNotification"))
                    this.Frame = new CGRect(0, ((parentHeight - 50) - bounds.Height), parentWidth, 50);
                else
                    this.Frame = new CGRect(0, parentHeight - 100, parentWidth, 50);
            }
            SetNeedsDisplay();
            UIView.CommitAnimations();
        }
    }

    internal class AccessoryViewTextFieldDelegate : UITextViewDelegate
    {
        public override bool ShouldBeginEditing(UITextView textView)
        {
            return true;
        }
        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            if (text.Equals("\n"))
            {

                textView.ResignFirstResponder();
                // Return FALSE so that the final '\n' character doesn't get added
                return false;
            }
            // For any other character return TRUE so that the text gets added to the view
            return true;
        }
    }

    public static class UIViewControllerExtensions
    {       
        /*
        Create an input view which sticks to keyboard.
        This function takes three argument for button names. 
        */
        public static KeyboardAccessoryView getInputAccessoryView(this UIViewController controller, UIView parentView, int numberOfButtons)
        {
            KeyboardAccessoryView accessoryView = KeyboardAccessoryView.GetView(parentView, numberOfButtons);
            return accessoryView;
        }
        /* 
        Perform Camera and PhotoGallery options methods
        input -> array of options to be taken into consideration max are considered (Used array incase more Generic logic in future)
        output-> picker view if only one option provided or actionsheet followed by pickerview for respective actions  
        */
        public static void performCameraAction(this UIViewController controller, string[] options, EventHandler<Models.UIImageModel> imagePicked)
        {
            if (options.Length > 1)
            {
                string title = "Select Photo";
                string subtitle = "Select one of the following options for selecting photos";
                createActionSheet(controller, title, subtitle, options, imagePicked);
            }
            else
            {
                takePhoto(controller, imagePicked); // only one option is present
            }

        }
        static void createActionSheet(UIViewController controller, string title, string subtitle, string[] options, EventHandler<Models.UIImageModel> imagePicked)
        {
            // Create a new Alert Controller
            UIAlertController actionSheetAlert = UIAlertController.Create(title, subtitle, UIAlertControllerStyle.ActionSheet);
            // Add Actions
            actionSheetAlert.AddAction(UIAlertAction.Create(options[0], UIAlertActionStyle.Default, (action) => takePhoto(controller, imagePicked)));
            actionSheetAlert.AddAction(UIAlertAction.Create(options[1], UIAlertActionStyle.Default, (action) => selectPhotoFromGallery(controller, imagePicked)));
            actionSheetAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Cancel")));

            // Required for iPad - We must specify a source for the Action Sheet since it is
            // displayed as a popover
            UIPopoverPresentationController presentationPopover = actionSheetAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = controller.View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the alert
            controller.PresentViewController(actionSheetAlert, true, null);
        }
        /*
        Function: Provides a functionality to take picture from rear camera 
        */
        static void takePhoto(UIViewController controller, EventHandler<Models.UIImageModel> imagePicked)
        {

            if (!UIImagePickerController.IsCameraDeviceAvailable(UIImagePickerControllerCameraDevice.Rear))
            {
                UIAlertView view = new UIAlertView("Error", "Camera not found or not ready", null, "OK", null);
                view.Show();
            }
            else
            {
                UIImagePickerController picker = new UIImagePickerController();
                picker.NavigationBar.BarTintColor = UIColor.FromRGBA(248, 155, 21, 1);
                PickerDelegate imagePickerDelegate = new PickerDelegate();
                imagePickerDelegate.imagePicked += imagePicked;
                picker.Delegate = imagePickerDelegate;
                picker.AllowsEditing = true;
                picker.SourceType = UIImagePickerControllerSourceType.Camera;
                controller.PresentModalViewController(picker, true);
            }

        }
        /*
        Function: Provides a functionality to select picture from gallery. 
        */
        static void selectPhotoFromGallery(UIViewController controller, EventHandler<Models.UIImageModel> imagePicked)
        {
            UIImagePickerController picker = new UIImagePickerController();
            picker.NavigationBar.BarTintColor = UIColor.FromRGBA(248, 155, 21, 1);
            PickerDelegate imagePickerDelegate = new PickerDelegate();
            imagePickerDelegate.imagePicked += imagePicked;
            picker.Delegate = imagePickerDelegate;    
            picker.AllowsEditing = true;
            picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
            controller.PresentModalViewController(picker, true);
        }

    }
    //Delegate Class to have the pickerDelegate methods
    public class PickerDelegate : UIImagePickerControllerDelegate
    {
        public event EventHandler<Models.UIImageModel> imagePicked; 
        public override void FinishedPickingImage(UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
        {
            //TODO: Add some logic that could return image from here.
            UIImage pickedImage;
            picker.DismissModalViewController(true);
            if(editingInfo[UIImagePickerController.EditedImage] != null)
            {
                pickedImage = editingInfo[UIImagePickerController.EditedImage] as UIImage;
            }
            else
            {
                pickedImage = editingInfo[UIImagePickerController.OriginalImage] as UIImage;
            }
            Models.UIImageModel newDataModel = new Models.UIImageModel();
            newDataModel.base64String = pickedImage.AsJPEG().GetBase64EncodedString(NSDataBase64EncodingOptions.None);
            imagePicked(this, newDataModel);
        }
        public override void Canceled(UIImagePickerController picker)
        {
            picker.DismissModalViewController(true);
        }
    }
}

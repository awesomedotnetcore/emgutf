﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __IOS__
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.TF.Lite;
using Emgu.Models;
using Emgu.TF.Lite.Models;
using UIKit;
using CoreGraphics;
using Xamarin.Forms;

using System.Threading;
using Foundation;
using AVFoundation;
using CoreVideo;
using CoreMedia;
using CoreImage;
using CoreFoundation;
using Xamarin.Forms.Platform.iOS;

namespace Emgu.TF.XamarinForms
{
    public class CameraViewPage : Xamarin.Forms.ContentPage
    {
        public UIImageView ImageView;
        private Label _label;
        AVCaptureSession session;
        OutputRecorder outputRecorder;
        DispatchQueue queue;

        private CocoSsdMobilenet _mobilenet;
        private string[] _imageFiles = null;

        public CameraViewPage()
           : base()
        {
            Button button = new Button();
            button.Text = "Button";
            _label = new Label();
            _label.Text = "Label";

            ImageView = new UIImageView();

            Xamarin.Forms.StackLayout stackLayout = new StackLayout();
            stackLayout.Children.Add(button);
            stackLayout.Children.Add(_label);
            stackLayout.Children.Add(ImageView.ToView());

            Content = stackLayout;

            CheckVideoPermissionAndStart();
        }


        private void CheckVideoPermissionAndStart()
        {
            AVFoundation.AVAuthorizationStatus authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            switch (authorizationStatus)
            {
                case AVAuthorizationStatus.NotDetermined:
                    AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, delegate (bool granted)
                    {
                        if (granted)
                        {
                            SetupCaptureSession();
                        }
                        else
                        {
                            _label.Text = "Please grant Video Capture permission";
                            //RenderImageMessage("Please grant Video Capture permission");
                        }
                    });
                    break;
                case AVAuthorizationStatus.Authorized:
                    SetupCaptureSession();
                    break;
                case AVAuthorizationStatus.Denied:
                case AVAuthorizationStatus.Restricted:
                    _label.Text = "Please grant Video Capture permission";
                    //RenderImageMessage("Please grant Video Capture permission");
                    break;
                default:

                    break;
                    //do nothing
            }

        }
        private void SetupCaptureSession()
        {
            // configure the capture session for low resolution, change this if your code
            // can cope with more data or volume
            session = new AVCaptureSession()
            {
                SessionPreset = AVCaptureSession.PresetMedium
            };

            // create a device input and attach it to the session
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);
            if (captureDevice == null)
            {
                //RenderImageMessage("Capture device not found.");
                _label.Text = "Capture device not found.";
                return;
            }
            var input = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (input == null)
            {
                //RenderImageMessage("No input device");
                _label.Text = "No input device";
                return;
            }
            session.AddInput(input);

            // create a VideoDataOutput and add it to the sesion
            AVVideoSettingsUncompressed settingUncomp = new AVVideoSettingsUncompressed();
            settingUncomp.PixelFormatType = CVPixelFormatType.CV32BGRA;
            var output = new AVCaptureVideoDataOutput()
            {
                UncompressedVideoSetting = settingUncomp,

                // If you want to cap the frame rate at a given speed, in this sample: 15 frames per second
                //MinFrameDuration = new CMTime (1, 15)
            };


            // configure the output
            queue = new DispatchQueue("myQueue");
            outputRecorder = new OutputRecorder(ImageView);
            output.SetSampleBufferDelegateQueue(outputRecorder, queue);
            session.AddOutput(output);

            session.StartRunning();

        }
    }

    public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private UIImageView _imageView;
        public OutputRecorder(UIImageView imageView)
        {
            _imageView = imageView;
        }
        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            try
            {
                UIImage image = ImageFromSampleBuffer(sampleBuffer);

                // Do something with the image, we just stuff it in our main view.
                BeginInvokeOnMainThread(delegate
                {
                    if (_imageView.Frame.Size != image.Size)
                        _imageView.Frame = new CGRect(CGPoint.Empty, image.Size);
                    _imageView.Image = image;
                });

                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                // 
                sampleBuffer.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //private static MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1.0, 1.0);

        UIImage ImageFromSampleBuffer(CMSampleBuffer sampleBuffer)
        {
            // Get the CoreVideo image
            using (CVPixelBuffer pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
            {
                // Lock the base address
                pixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
                CIImage cIImage = new CIImage(pixelBuffer);
                pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
                return new UIImage(cIImage);
            }
        }
    }
}

#endif
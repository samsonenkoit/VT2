using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VtApp.Controls;

public partial class LevelBar : UserControl
{
    private static readonly Color[] LevelColors =
    [
        Color.FromRgb(0x42, 0xA5, 0xF5), // Blue 400
        Color.FromRgb(0x66, 0xBB, 0x6A), // Green 400
        Color.FromRgb(0xFF, 0xA7, 0x26), // Orange 400
        Color.FromRgb(0xEF, 0x53, 0x50), // Red 400
    ];

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(int),
        typeof(LevelBar),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnLayoutChanged));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(int),
        typeof(LevelBar),
        new PropertyMetadata(2, OnLayoutChanged));

    public static readonly DependencyProperty BarOrientationProperty = DependencyProperty.Register(
        nameof(BarOrientation),
        typeof(Orientation),
        typeof(LevelBar),
        new PropertyMetadata(Orientation.Vertical, OnLayoutChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public Orientation BarOrientation
    {
        get => (Orientation)GetValue(BarOrientationProperty);
        set => SetValue(BarOrientationProperty, value);
    }

    public LevelBar()
    {
        InitializeComponent();
        Loaded += (_, _) => Rebuild();
        SizeChanged += (_, _) => UpdateThumb();
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LevelBar bar && bar.IsLoaded)
        {
            if (e.Property == ValueProperty)
                bar.UpdateThumb();
            else
                bar.Rebuild();
        }
    }

    private int LevelCount => Math.Max(2, Maximum + 1);

    private void Rebuild()
    {
        SegmentsHost.Children.Clear();
        SegmentsHost.RowDefinitions.Clear();
        SegmentsHost.ColumnDefinitions.Clear();

        if (BarOrientation == Orientation.Vertical)
        {
            Root.Width = 20;
            Root.ClearValue(HeightProperty);
            Root.MinHeight = 140;
            Root.HorizontalAlignment = HorizontalAlignment.Center;
            Root.VerticalAlignment = VerticalAlignment.Stretch;
            Thumb.Width = 16;
            Thumb.Height = 16;

            for (var i = 0; i < LevelCount; i++)
                SegmentsHost.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }
        else
        {
            Root.Height = 12;
            Root.ClearValue(WidthProperty);
            Root.MinWidth = 120;
            Root.HorizontalAlignment = HorizontalAlignment.Stretch;
            Root.VerticalAlignment = VerticalAlignment.Center;
            Thumb.Width = 16;
            Thumb.Height = 16;

            for (var i = 0; i < LevelCount; i++)
                SegmentsHost.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (var i = 0; i < LevelCount; i++)
        {
            var level = BarOrientation == Orientation.Vertical
                ? LevelCount - 1 - i
                : i;

            var segment = new Border
            {
                Background = new SolidColorBrush(ColorForLevel(level)),
                Cursor = Cursors.Hand,
                Tag = level,
            };

            if (BarOrientation == Orientation.Vertical)
                Grid.SetRow(segment, i);
            else
                Grid.SetColumn(segment, i);

            SegmentsHost.Children.Add(segment);
        }

        UpdateThumb();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SetValueFromPosition(e.GetPosition(Root));
        Root.CaptureMouse();
        e.Handled = true;
    }

    private void Root_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && Root.IsMouseCaptured)
            SetValueFromPosition(e.GetPosition(Root));
    }

    private void Root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (Root.IsMouseCaptured)
            Root.ReleaseMouseCapture();
    }

    private void SetValueFromPosition(Point position)
    {
        int level;
        if (BarOrientation == Orientation.Vertical)
        {
            if (Root.ActualHeight <= 0)
                return;
            var ratio = Math.Clamp(position.Y / Root.ActualHeight, 0, 0.999);
            level = LevelCount - 1 - (int)(ratio * LevelCount);
        }
        else
        {
            if (Root.ActualWidth <= 0)
                return;
            var ratio = Math.Clamp(position.X / Root.ActualWidth, 0, 0.999);
            level = (int)(ratio * LevelCount);
        }

        Value = Math.Clamp(level, 0, LevelCount - 1);
    }

    private void UpdateThumb()
    {
        if (!IsLoaded)
            return;

        var value = Math.Clamp(Value, 0, LevelCount - 1);

        if (BarOrientation == Orientation.Vertical)
        {
            if (Root.ActualHeight <= 0)
                return;

            var segmentHeight = Root.ActualHeight / LevelCount;
            var indexFromTop = LevelCount - 1 - value;
            Canvas.SetLeft(Thumb, (Root.ActualWidth - Thumb.Width) / 2);
            Canvas.SetTop(Thumb, indexFromTop * segmentHeight + segmentHeight / 2 - Thumb.Height / 2);
        }
        else
        {
            if (Root.ActualWidth <= 0)
                return;

            var segmentWidth = Root.ActualWidth / LevelCount;
            Canvas.SetLeft(Thumb, value * segmentWidth + segmentWidth / 2 - Thumb.Width / 2);
            Canvas.SetTop(Thumb, (Root.ActualHeight - Thumb.Height) / 2);
        }

        Thumb.Fill = new SolidColorBrush(ColorForLevel(value));
    }

    private Color ColorForLevel(int level)
    {
        // 3-level bars skip blue so they read as green → orange → red.
        var index = LevelCount == 3 ? level + 1 : level;
        return LevelColors[Math.Clamp(index, 0, LevelColors.Length - 1)];
    }
}

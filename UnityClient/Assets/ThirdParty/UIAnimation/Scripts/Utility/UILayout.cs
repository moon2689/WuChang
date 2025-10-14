namespace SakashoUISystem
{
    public enum UIChildAlignment
    {
        UpperLeft,
        MiddleCenter
    }
    
    public enum UIAxis
    {
        Vertical,
        Horizontal
    }
    
    public interface IUILayout
    {
        float Top { get; set; }
        float Left { get; set; }
        float Width { get; set; }
        float Height { get; set; }

        float Bottom { get; }
        float Right { get; }
    }

    public class UILayout : IUILayout
    {
        public float Bottom {
            get {
                return Top - Height;
            }
        }
        
        public float Right {
            get {
                return Left + Width;
            }
        }
        
        private float top;
        public float Top {
            get {
                return top;
            }
            set {
                top = value;
            }
        }
        
        private float width;
        public float Width {
            get {
                return width;
            }
            set {
                width = value;
            }
        }
        
        private float height;
        public float Height {
            get {
                return height;
            }
            set {
                height = value;
            }
        }
        
        private float left;
        public float Left {
            get {
                return left;
            }
            set {
                left = value;
            }
        }

        public UILayout(float left, float top, float width, float height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }
}
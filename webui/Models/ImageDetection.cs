namespace webui.Models
{
    public class ImageDetection
    {
        public int model { get; set; } = 0;
        public IFormFile uploadImage { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string conf { get; set; }
        public string iou { get; set; }
    }
}

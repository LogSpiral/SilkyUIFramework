namespace SilkyUIFramework;

//public class UIElementHook : ModSystem
//{
//    public override void Load()
//    {
//        On_UIElement.Append += (orig, self, element) =>
//        {
//            if (self is View view)
//                view.AppendFromView(view);
//            else orig(self, element);
//        };

//        On_UIElement.GetElementAt += (orig, self, point) =>
//        {
//            if (self is View view)
//                return view.GetElementAtFromView(point);
//            return orig(self, point);
//        };
//    }
//}
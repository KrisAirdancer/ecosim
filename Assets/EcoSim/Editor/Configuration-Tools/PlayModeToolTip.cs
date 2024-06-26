using UnityEngine;
using UnityEngine.UIElements;

public class PlayModeToolTip : Manipulator
{

    private VisualElement tooltip;
    private VisualElement root;

    public PlayModeToolTip(VisualElement root)
    {
        this.root = root;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseEnterEvent>(MouseOver);
        target.RegisterCallback<MouseOutEvent>(MouseOff);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseEnterEvent>(MouseOver);
        target.UnregisterCallback<MouseOutEvent>(MouseOff);
    }

    private void MouseOver(MouseEnterEvent e)
    {

        if (tooltip == null)
        {
            tooltip = new VisualElement();
            tooltip.style.position = Position.Absolute;
            tooltip.style.right = this.target.worldBound.xMin;
            tooltip.style.left = this.target.worldBound.xMin;
            tooltip.style.top = this.target.worldBound.yMin;
            tooltip.style.backgroundColor = Color.black;
            var label = new Label(this.target.tooltip);
            label.style.color = Color.white;
            label.style.whiteSpace = WhiteSpace.Normal;
            tooltip.Add(label);
            root.Add(tooltip);
            tooltip.style.visibility = Visibility.Hidden;
        }
        if (Application.isPlaying)
        {
            tooltip.style.visibility = Visibility.Visible;
            tooltip.BringToFront();
        }
    }

    private void MouseOff(MouseOutEvent e)
    {
        tooltip.style.visibility = Visibility.Hidden;
    }
}
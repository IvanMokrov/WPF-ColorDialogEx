# WPF ColorDialogEx
## ColorDialog with Harmonic colors selection helpers and HSL feautures

### Small version
This ColorDicalog have a simple "small" **dark theme** and **light theme** versions, which have a color chooser, **Alpha** component slider (upside) and **Saturation** slider (downside).
Each small triangle means a **harmonic color** for the current choosen color - just click it.  

![smalllight](https://user-images.githubusercontent.com/22683821/48414705-7de57f80-e75c-11e8-9397-4c1963f00c41.png) ![smalldark](https://user-images.githubusercontent.com/22683821/48414712-81790680-e75c-11e8-8c99-0329e99952f7.png)

A cube on the left side shows a current selected color on the front, and a prevous selected color on the sides. It may helps you to preview a harmonics of colors.
You can manually copy/paste values of HSLA and ARGB

### Big Version
Also, this project contains a more feautured "big verion" of dialog:

![biglight](https://user-images.githubusercontent.com/22683821/48415385-5099d100-e75e-11e8-8fe9-610cb3cc8453.png)

It have a set of "slots" for predefined color sets (can put to slot from front cube color, and load to front and back cube color).
This interactive slots autosave to default .palette file in the **\User\Documents\Palettes\** folder. Also, you can save sets to custom palette files (two triangle buttons over OK button)

Also it have "ColorSet mode". It enable a color choice via mouse moving over a colors and collect it medians to selection, like a reallife artist's palette.

### Test app
And finally, this project contains a small test application, where you can play with this control and try your harmonic set of colors with brush.

![testapp](https://user-images.githubusercontent.com/22683821/48414671-70c89080-e75c-11e8-9cc2-c59c02305573.png)

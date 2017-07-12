---
---
# Creating a foreground application

## Introduction

Now we will create a blank foreground application. Features will be added as we progress through other tutorials.

## Using the blank app template

Right click your solution and choose *Add > New Project...*. Choose *Visual C# > Windows Universal* and name it *Showcase*.

![New project.png](NewProject.png)

* We will create a navigation pane and a `Frame` in the main page. The `Frame` will show the main content to the user and the navigation pane will allow navigation between pages.

![Navigation pane](NavigationPane.png)

* The layout is made of a `SplitView` containing `StackPanel`s for the buttons in its `Pane` and a `Frame` in its `Content`, named `ContentFrame`, that will keep the current page. [The XAML can be seen here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/Views/MainPage.xaml)

* Whenever a button is clicked, we should move to the corresponding page. Furthermore, since it's a flat navigation tree, we don't want to keep the `BackStack` history (the history of pages to return to whenever the back button is pressed). The following function hides the `Pane` and navigates to the new page, clearing the `BackStack`:

```cs
private void ContentNavigate(Type page)
{
    Splitter.IsPaneOpen = false;
    if (ContentFrame.CurrentSourcePageType != page)
    {
        ContentFrame.Navigate(page);
        ContentFrame.BackStack.Clear();
    }
}
```

* Now, it's just a matter of creating the callbacks for button presses that call `ContentNavigate` with the target page. For example, to navigate to the `SlideShow` when the button is pressed, do:

```cs
private void SlideShow_Click(object sender, RoutedEventArgs e)
{
    ContentNavigate(typeof(SlideShow));
}
```

* A hamburger button allows the user to show or collapse the panel. The code for the button click is:

```cs
private void PanelToggle_Click(object sender, RoutedEventArgs e)
{
    Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
}
```

* [The full code for the MainPage can be seen here.](https://github.com/ms-iot/iot-walkthrough/blob/master/CS/Showcase/Views/MainPage.xaml.cs)

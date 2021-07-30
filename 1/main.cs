//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------
/* Types for inspector.
TypePoint2FVector
TypeS8
TypeS32Vector
TypeBoolVector
TypeF32Vector
TypeStringTableEntryVector
TypeCaseString
TypeFilename
TypeSimObjectName
*/

function GuiEditorToy::create( %this )
{
	exec("./assets/gui/guiEditorCtrl.gui");
	exec("./assets/gui/guiToolbox.gui");
	exec("./assets/gui/ColorSelectorTool.gui");
	exec("./assets/scripts/CollisionArrayBuilder.cs");
	//exec("./assets/scripts/GuiEditorCtrlProperties.cs");
	exec("./assets/scripts/GuiEditorUndo.cs");
    // Set the sandbox drag mode availability.
    Sandbox.allowManipulation( pan );
    Sandbox.allowManipulation( pull );
    
    // Set the manipulation mode.
    Sandbox.useManipulation( pull );
	SandboxScene.clear();
    //SandboxWindow.delete();
	GuiEditorToy.blankGui = new GuiControl();
	
	// Init menu
	GuiEditorToy.initMenus();
	
    // Reset the toy initially.
    GuiEditorToy.reset();
	
}

function GuiEditorToy::initMenus( %this )
{
   if( isObject( %this.menuGroup ) )
      %this.menuGroup.delete();
      
   %this.menuGroup = new SimGroup();
   
   //set up %cmdctrl variable so that it matches OS standards
   %cmdCtrl = $platform $= "macos" ? "Cmd" : "Ctrl";

   %filemenu = new PopupMenu()
   {
      superClass = "MenuBuilder";
      barPosition = 0;
      barName = "File";
      
      item[0] = "New Gui..." TAB %cmdCtrl SPC "N" TAB "GuiEditorStartCreate();";
      item[1] = "Open Gui..." TAB %cmdCtrl SPC "O" TAB "GuiEditorToy.openGui();";
      item[2] = "-";
      item[3] = "Save Gui..." TAB %cmdCtrl SPC "S" TAB "GuiEditorToy.save();";
   };
   
   %editmenu = new PopupMenu()
   {
      superClass = "MenuBuilder";
      barPosition = 1;
      barName = "Edit";
      
      item[0] = "Undo" TAB %cmdCtrl SPC "Z" TAB "GuiEditor.undo();";
      item[1] = "Redo" TAB %cmdCtrl @ "-shift Z" TAB "GuiEditor.redo();";
   };

   %layoutmenu = new PopupMenu()
   {
      superClass = "MenuBuilder";
      barPosition = 2;
      barName = "Layout";
      
      item[0] = "Align Left" TAB %cmdCtrl SPC "L" TAB "GuiEditor.Justify(0);";
      item[1] = "Align Right" TAB %cmdCtrl SPC "R" TAB "GuiEditor.Justify(2);";
      item[2] = "Align Top" TAB %cmdCtrl SPC "T" TAB "GuiEditor.Justify(3);";
      item[3] = "Align Bottom" TAB %cmdCtrl SPC "B" TAB "GuiEditor.Justify(4);";
      item[4] = "-";
      item[5] = "Center Horizontally" TAB "" TAB "GuiEditor.Justify(1);";
      item[6] = "Space Vertically" TAB "" TAB "GuiEditor.Justify(5);";
      item[7] = "Space Horizontally" TAB "" TAB "GuiEditor.Justify(6);";
      item[8] = "-";
      item[9] = "Bring to Front" TAB "" TAB "GuiEditor.BringToFront();";
      item[10] = "Send to Back" TAB "" TAB "GuiEditor.PushToBack();";
      item[11] = "Lock Selection" TAB "" TAB "GuiEditorTreeView.lockSelection(true);";
      item[12] = "Unlock Selection" TAB "" TAB "GuiEditorTreeView.lockSelection(false);";
   };
   
   %movemenu = new PopupMenu()
   {
      superClass = "MenuBuilder";
      barPosition = 3;
      barName = "Move";
      
      item[0] = "Nudge Up" TAB "Up" TAB "GuiEditor.moveSelection(0,-1);";
      item[1] = "Nudge Down" TAB "Down" TAB "GuiEditor.moveSelection(0,1);";
	  item[2] = "Nudge Right" TAB "Right" TAB "GuiEditor.moveSelection(1,0);";
	  item[3] = "Nudge Left" TAB "Left" TAB "GuiEditor.moveSelection(-1,0);";
   };
   
   // add menus to a group
   %this.menuGroup.add(%fileMenu);
   %this.menuGroup.add(%editmenu);
   %this.menuGroup.add(%layoutmenu);
   %this.menuGroup.add(%movemenu);
   // save this for adding to.
   %this.menuGroup.editMenu = %editMenu;
   
   for( %i = 0; %i < %this.menuGroup.getCount(); %i++ )
     %this.menuGroup.getObject( %i ).attachToMenuBar();
   
}

//-----------------------------------------------------------------------------

function GuiEditorToy::destroy( %this )
{   
}

//-----------------------------------------------------------------------------

function GuiEditorToy::reset(%this)
{
    // Clear the scene.
    
	Canvas.setContent(GuiEditorCtrl);
	
	GuiEditorCtrl.add(GuiTreeViewWindow);
	%this.loadGui(GuiEditorToy.blankGui);
	
	$GuiRootElement = GuiEditorToy.blankGui;
	%rootSize = $GuiRootElement.extent;
	
    GuiEditorCtrl.add(GuiToolbox);
    GuiEditorCtrl.add(GuiInspectorWindow);
    
    //GuiEditorCtrl.add(CBBWindow);
	
}

function GuiEditorToy::openGui(%this)
{
	%guiLoad = new OpenFileDialog();
	%guiLoad.DefaultPath = "modules/";
	%guiLoad.Title = "Open Gui";
	%guiLoad.MustExist = true;
	%guiLoad.ChangePath = false;
	%guiLoad.Filters = "GUI Files(*.gui)";
	%newFile = "";
	if(%guiLoad.Execute())
	{
		%newFile = %guiLoad.fileName;
		%ext = fileExt(%newFile);
		if(%ext !$= ".gui")
		{
			echo("wrong file format");
			return;
		}
		echo(%newFile);
	}
	%guiLoad.delete();
	%this.loadGui(%newFile);
	
}

function GuiEditorToy::loadGui(%this, %guiContent)
{
	// create a fallback
	if(!isObject(GuiEditorToy.blankgui)) 
		GuiEditorToy.blankgui = new GuiControl();
  
	while((%obj = GuiEditorContent.getObject(0)) != -1)
		GuiGroup.add(%obj);
  
	// exec the file if we are loading one.
	if(fileExt(%guiContent) $= ".gui")
		exec(%guiContent);
	
	// add whatever is in guiContent to the scene.
	GuiEditorContent.add(%guiContent);
	%ext = GuiEditorContent.getExtent();
	GuiEditorContent.getObject(0).resize(0,0,getWord(%ext,0), getWord(%ext, 1));
	
	$GuiRootElement = %guiContent.getId();
	%rootSize = $GuiRootElement.extent;
	
	GuiEditor.setRoot(%guiContent);
	GuiEditorTreeView.open(%guiContent);
	
	GuiEditor.getUndoManager().clearAll();
	
	GuiEditor.setFirstResponder();
	GuiEditor.setSnapToGrid(8);
	GuiEditor.updateUndo();
	GuiEditor.lastFile = %guiContent;
	$pref::GuiEditor::lastFile = %guiContent;
}

function GuiEditor::undo(%this)
{
	%this.getUndoManager().undo();
	%this.updateUndo();
	%this.clearSelection();
}

function GuiEditor::redo(%this)
{
	%this.getUndoManager().redo();
	%this.updateUndo();
	%this.clearSelection();
}

function GuiEditor::updateUndo(%this)
{
	%man = %this.getUndoManager();
	%nextUndo = %man.getNextUndoName();
	%nextRedo = %man.getNextRedoName();

	%cmdCtrl = $platform $= "macos" ? "Cmd" : "Ctrl";
	%undoitem = "Undo" SPC %nextUndo TAB %cmdCtrl SPC "Z" TAB "GuiEditor.undo();";
	%redoitem = "Redo" SPC %nextRedo TAB %cmdCtrl @ "-shift Z" TAB "GuiEditor.redo();";
   
	GuiEditorToy.menuGroup.editMenu.removeItem(0);
	GuiEditorToy.menuGroup.editMenu.removeItem(0);
	GuiEditorToy.menuGroup.editMenu.addItem(0, %undoitem);
	GuiEditorToy.menuGroup.editMenu.addItem(1, %redoitem);
   
	GuiEditorToy.menuGroup.editMenu.enableItem(0, %nextUndo !$= "");
	GuiEditorToy.menuGroup.editMenu.enableItem(1, %nextRedo !$= "");
}

function GuiEditorToy::save(%this)
{
	%guiObj = GuiEditorContent.getObject(0);
	if(%guiObj == -1)
	{
		return;
	}
	
	if(%guiObj.getName() !$= "")
	{
		%name = %guiObj.getName() @ ".gui";
	}
	else
	{
		%name = "untitled.gui";
	}
	
	%fo = new FileObject();
	%fo.openForWrite(%name);
	%fo.writeLine("//--- Created With GUIEDITORTOY ---//");
	%fo.writeObject(%guiObj, "%guiContent = ");
	%fo.writeLine("//--- GUIEDITORTOY END ---//");
	%fo.close();
	%fo.delete();
	
}

//-----------------------------------------------------------------------------

function GuiToolbox::onWake(%this)
{
	%controls = enumerateConsoleClasses("GuiControl");
	%this-->toolboxList.clearItems();
	for(%i = 0; %i < getFieldCount(%controls); %i++)
	{
		%field = getField(%controls, %i);
		if(%field $= "GuiCanvas")
			continue;
		
		%this-->toolboxList.addItem(%field);
	}
}

/*
hold this for me.
new GuiChainCtrl(GuiEditorCtrlProperties) {
                isContainer = "1";
                Position = "0 0";
                Extent = "340 40";
				HorizSizing = "width";
				VertSizing = "height";
            };

*/

//-----------------------------------------------------------------------------

function GuiEditorToolboxDrag::onTouchDragged(%this, %index, %text)
{
	%position = %this.getGlobalPosition();
	%cursorpos = Canvas.getCursorPos();
   
	%class = %this.getItemText(%this.getSelectedItem());
	%payload = eval("return new " @ %class @ "();");
	if(!isObject(%payload))
		return;
      
	%xOffset = getWord(%payload.extent, 0) / 2;
	%yOffset = getWord(%payload.extent, 1) / 2; 
	
	// position where the drag will start, to prevent visible jumping.
	%xPos = getWord(%cursorpos, 0) - %xOffset;
	%yPos = getWord(%cursorpos, 1) - %yOffset;
	   
	%dragCtrl = new GuiDragAndDropCtrl() {
		canSaveDynamicFields = "0";
		Profile = "GuiDragAndDropProfile";
		HorizSizing = "right";
		VertSizing = "bottom";
		Position = %xPos SPC %yPos;
		extent = %payload.extent;
		MinExtent = "32 32";
		canSave = "1";
		Visible = "1";
		hovertime = "1000";
		Text = %text;
		deleteOnMouseUp = true;
	};

	%dragCtrl.add(%payload);
	Canvas.getContent().add(%dragCtrl);
	   
	%dragCtrl.startDragging(%xOffset, %yOffset);
}

//-----------------------------------------------------------------------------

function GuiEditor::onControlDragged(%this, %payload, %position)
{
	%pos = VectorSub(%position, GuiEditorContent.getGlobalPosition());
	%x = getWord(%pos, 0);
	%y = getWord(%pos, 1);
	%target = GuiEditorContent.findHitControl(%x, %y);
	
	while(! %target.isContainer )
	{
		%target = %target.getParent();
	}
	
	if(%target != %this.getCurrentAddset())
	{
		%this.setCurrentAddSet(%target);
	}
	GuiEditorCtrlProperties.update(%ctrl);
}

//-----------------------------------------------------------------------------

function GuiEditor::onControlDropped(%this, %payload, %position)
{  
   %pos = %payload.getGlobalPosition();
   %x = getWord(%pos, 0);
   %y = getWord(%pos, 1);
   
   if(%x > $GuiRootElement.extent.x || %y > $GuiRootElement.extent.y)
   {
      messageBox("Error", "Cannot add a control outside the root gui element!");
      return;
   }
   
   %this.addNewCtrl(%payload);
   
   %payload.setPositionGlobal(%x, %y);
   %this.setFirstResponder();
}

//-----------------------------------------------------------------------------

function GuiEditorCtrlProperties::update(%this, %inspectTarget)
{
	GuiEditorCtrlProperties.inspect(%inspectTarget);
}

function GuiEditor::onSelect(%this, %ctrl)
{
	GuiEditorCtrlProperties.update(%ctrl);
	GuiEditor.clearSelection();
	GuiEditor.select(%ctrl);
	GuiEditorTreeView.addSelection(%ctrl);
}

function GuiEditor::onSelectionMoved(%this, %ctrl)
{
	GuiEditorCtrlProperties.update(%ctrl);
}

function GuiEditor::onClearSelected(%this)
{ 
   GuiEditorTreeView.clearSelection();
}

function GuiEditor::onSelectionParentChange(%this)
{
	GuiEditorTreeView.update();
}

function GuiEditor::onDelete(%this)
{
	GuiEditorTreeView.update();
	GuiEditorCtrlProperties.update(0);
}

function GuiEditor::onAddSelected(%this,%ctrl)
{
   GuiEditorTreeView.addSelection(%ctrl);
   GuiEditorTreeView.scrollVisibleByObjectId(%ctrl);
}

function GuiEditor::onRemoveSelected(%this,%ctrl)
{
   GuiEditorTreeView.removeSelection(%ctrl); 
}

//-----------------------------------------------------------------------------

function GuiEditorTreeView::update(%this)
{
	%obj = GuiEditorContent.getObject(0);
	
	if(!isObject(%obj))
	{
		GuiEditorTreeView.clear();
	}
	else
	{
		GuiEditorTreeView.open(GuiEditorContent.getObject(0));
	}
}

function GuiEditorTreeView::onRightMouseDown(%this, %item, %pts, %obj)
{
   if(%obj)
   {
      GuiEditor.setCurrentAddSet(%obj);
   }
}

function GuiEditorTreeView::onAddSelection(%this, %ctrl)
{
	GuiEditor.setFirstResponder();
}

function GuiEditorTreeView::onRemoveSelection(%this, %ctrl)
{
	GuiEditor.removeSelection(%ctrl);
}

function GuiEditorTreeView::onDeleteSelection(%this)
{ 
   GuiEditor.clearSelection();
}

function GuiEditorTreeView::onSelect(%this, %obj)
{

   if(isObject(%obj))
   {
      GuiEditorCtrlProperties.update(%obj);
      GuiEditor.select(%obj);
   }
}






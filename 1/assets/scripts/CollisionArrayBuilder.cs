///////////////////////////////////////////////////////////////
// THIS FILE IS IMPORTANT FOR EDITORS!!!!
// the only time this file should be removed
// is when these functions are added to source.
///////////////////////////////////////////////////////////////

function CollisionArrayUpdate(%obj,%field, %id, %chkId, %callback)
{
	
	%on = %chkId.getValue();
	%newData = %obj.getFieldValue(%field);
	%find = false;
	%count = getWordCount(%newData);
	for(%i = 0; %i < %count; %i++)
	{
		%val = getWord(%newData,%i);
		if(%val $= %id)
		{
			%find = true;
			if(!%on)
			{
				%newData = removeWord(%newData, %i);
				break;
			}
		}
				
	}
	
	// colllayers and groups don't care what order they are in.
	if(%on && !%find)
	{
		%newData = %newData SPC %id;
	}
	
	eval(%callback @ "(\"" @ %newData @ "\");" );

}

function ApplyPoint2Value(%callback,%xField, %yField)
{
	%xVal = %xField.getText();
	%yVal = %yField.getText();
	
	eval(%callback @ "(\"" @ %xVal SPC %yVal @ "\");");
}

function UpdatePoint2Value(%newData, %xField, %yField)
{
	// sets the text update for Point2I inspector fields.
	%count = getWordCount(%newData);
	for(%i = 0; %i < %count; %i++)
	{
		if(%i == 0)
		{
			%xField.setText(getWord(%newData, %i));
		}
		else
		{
			%yField.setText(getWord(%newData, %i));
		}
				
	}
}
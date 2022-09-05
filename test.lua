buttonOne = false;
buttonTwo = false;
buttonThree = false;
function buttonOnePressed()
    buttonOne = true;
end
function buttonTwoPressed()
    buttonTwo = true;
end
function buttonThreePressed()
    buttonThree = true;
end
function onStart(creator)
    cuboid = creator.createCuboid("test", Vector3.__new(33.97171,-319.3138, 32.18883), Vector3.__new(-10,10,10));
    button1 = creator.createMarker("button1", Vector3.__new(23.88107,-289.9126,35.43097), Vector3.__new(90,0,180));
    button1.interacted.add(buttonOnePressed);
    button2 = creator.createMarker("button2", Vector3.__new(4.48517,-300.5934,35.18379), Vector3.__new(90,0,75));
    button2.interacted.add(buttonTwoPressed);
    button3 = creator.createMarker("button3", Vector3.__new(21.91974,-326.3877,35.18382), Vector3.__new(90,0,-30));
    button3.interacted.add(buttonThreePressed);
end

function update()
    if buttonTwo then
        if buttonOne then
            if buttonThree then
                buttonOne = false;
                buttonTwo = false;
                buttonThree = false;
                cuboid.test();
            end
        else
            buttonThree = false;
        end
    else 
        buttonOne = false;
        buttonThree = false;
    end
end

-- xpos="23.88107" ypos="34.43097" zpos="-290.6126"
-- xpos="5.38517" ypos="34.18379" zpos="-300.5934"
-- xpos="21.91974" ypos="34.18382" zpos="-325.3877"

function onEmote()
    if cuboid.characterInside then
        cuboid.test();
    end
end

function onStarted()
    character.emoteUsed.add(onEmote);
end
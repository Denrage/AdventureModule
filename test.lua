cuboid = createCuboid("test", Vector3.__new(-740,512,8), Vector3.__new(10,10,10));
-- function update()
--     if cuboid.characterInside then
--         cuboid.test();
--     end
-- end


function onCuboidEntered()
    cuboid.test();
end

cuboid.playerEntered.add(onCuboidEntered);
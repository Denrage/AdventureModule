import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Guid } from "guid-typescript";
import { AdventureService, AdventureGroup, Adventure } from '../adventure.service';
import { PlayerService } from '../player.service';

@Component({
  selector: 'app-adventure',
  templateUrl: './adventure.component.html',
  styleUrls: ['./adventure.component.css']
})
export class AdventureComponent implements OnInit {
  adventure: Adventure | null = null;

  groups: AdventureGroup[] = [];

  constructor(private adventureService: AdventureService, private route: ActivatedRoute, private playerService: PlayerService) { }

  ngOnInit(): void {
    var id = this.route.snapshot.paramMap.get('id');
    if (id !== null) {
      this.adventure = this.adventureService.getAdventure(Guid.parse(id));
      this.adventure.groups.forEach(x => this.groups.push(this.adventureService.getAdventureGroup(x)));
    }
  }

}

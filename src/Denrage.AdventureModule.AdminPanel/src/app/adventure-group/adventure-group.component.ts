import { Component, Input, OnInit } from '@angular/core';
import { Guid } from "guid-typescript";
import { AdventureGroup, AdventureService } from '../adventure.service';
import { Player, PlayerService } from '../player.service';

@Component({
  selector: 'app-adventure-group',
  templateUrl: './adventure-group.component.html',
  styleUrls: ['./adventure-group.component.css']
})
export class AdventureGroupComponent implements OnInit {

  @Input()
  groupId: Guid = Guid.createEmpty();

  group: AdventureGroup | null = null;
  players: Player[] = [];

  constructor(private adventureService: AdventureService, private playerService: PlayerService) { }

  ngOnInit(): void {
    this.group = this.adventureService.getAdventureGroup(this.groupId);
    this.group.players.forEach(x => this.players.push(this.playerService.getPlayer(x)));
  }

}

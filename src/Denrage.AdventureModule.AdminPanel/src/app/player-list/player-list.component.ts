import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Guid } from 'guid-typescript';
import { EditPlayerDialogComponent } from '../edit-player-dialog/edit-player-dialog.component';
import { Player, PlayerService } from '../player.service';

@Component({
  selector: 'app-player-list',
  templateUrl: './player-list.component.html',
  styleUrls: ['./player-list.component.css']
})
export class PlayerListComponent implements OnInit {

  players: Player[] = [];

  constructor(private playerService: PlayerService, private dialog: MatDialog) { }

  ngOnInit(): void {
    this.players = this.playerService.players;
  }

  addPlayer(): void {
    var dialogRef = this.dialog.open(EditPlayerDialogComponent, {
      width: "250px",
      data: { player: { id: Guid.create(), name: "", image: "" } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.playerService.addPlayer(result.player);
      }
    })
  }

}

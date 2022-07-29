import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Player } from '../player.service';

@Component({
  selector: 'app-edit-player-dialog',
  templateUrl: './edit-player-dialog.component.html',
  styleUrls: ['./edit-player-dialog.component.css']
})
export class EditPlayerDialogComponent implements OnInit {
  
  constructor(@Inject(MAT_DIALOG_DATA) public data: { player: Player }, public dialogRef: MatDialogRef<EditPlayerDialogComponent>) { }

  ngOnInit(): void {
  }

  save(): void {
    this.dialogRef.close({ player: this.data.player });
  }

  close(): void {
    this.dialogRef.close();
  }

}

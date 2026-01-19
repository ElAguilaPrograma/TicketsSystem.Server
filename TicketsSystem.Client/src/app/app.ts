import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DarkModeService } from './core/services/dark-mode.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit{
  protected readonly title = signal('TicketsSystem.Client');

    constructor(private darkModeService: DarkModeService) { }
  
    ngOnInit(): void {
      this.darkModeService.ngOnInit();
    }
}

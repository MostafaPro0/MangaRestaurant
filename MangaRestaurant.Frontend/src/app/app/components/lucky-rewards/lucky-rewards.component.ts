import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateService } from '../../services/translate.service';
import { LuckyRewardsService, LuckyRewardsDrawResult } from '../../services/lucky-rewards.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-lucky-rewards',
  standalone: true,
  imports: [CommonModule, ButtonModule, TranslateModule],
  templateUrl: './lucky-rewards.component.html',
  styleUrl: './lucky-rewards.component.css'
})
export class LuckyRewardsComponent implements OnInit {
  userCoins: number = 0;
  isDrawing: boolean = false;
  showCapsule: boolean = false;
  isOpening: boolean = false;
  showPrize: boolean = false;
  
  wonColor: string = '#ef4444';
  currentPrize: LuckyRewardsDrawResult | null = null;
  isEnabled: boolean = false;
  
  @ViewChild('capsuleContainer') capsuleContainer!: ElementRef;

  constructor(
    public translateService: TranslateService,
    private luckyRewardsService: LuckyRewardsService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.refreshStatusAndCoins();
  }

  refreshStatusAndCoins() {
    this.luckyRewardsService.getStatus().subscribe(res => {
      this.isEnabled = res.isEnabled;
      if (this.isEnabled) {
        this.luckyRewardsService.getUserCoins().subscribe(coins => {
          this.userCoins = coins;
        });
      }
    });
  }

  getColors(index: number): string[] {
    const colors = [
      ['#ef4444', '#b91c1c'], // red
      ['#f59e0b', '#b45309'], // yellow
      ['#10b981', '#047857'], // green
      ['#3b82f6', '#1d4ed8'], // blue
      ['#a855f7', '#7e22ce'], // purple
      ['#ec4899', '#be185d'], // pink
    ];
    return colors[index % colors.length];
  }

  drawReward() {
    if (this.isDrawing || this.userCoins < 1 || !this.isEnabled) return;
    
    this.isDrawing = true;
    
    this.luckyRewardsService.drawReward().subscribe({
      next: (result) => {
        this.currentPrize = result;
        this.userCoins = result.remainingCoins;
        this.wonColor = result.color || '#ef4444';

        // Animate
        setTimeout(() => {
          this.showCapsule = true;
          
          setTimeout(() => {
            this.isOpening = true;
            
            setTimeout(() => {
              this.showPrize = true;
              this.isDrawing = false;
            }, 500);
            
          }, 1000);
        }, 1500);
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to draw' });
        this.isDrawing = false;
      }
    });
  }

  closePrize() {
    this.showPrize = false;
    this.showCapsule = false;
    this.isOpening = false;
    this.currentPrize = null;
  }
}

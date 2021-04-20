<template>
	<portfolio-settings-view />
	<portfolio-accounts-view />
	<portfolio-exchange-view />
	<portfolio-currency-view v-for="balance in balances" :key="balance.currency" :currency-code="balance.currency" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioCurrencyView from '@/view/portfolio/portfolioCurrencyView.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import PortfolioSettingsView from '@/view/portfolio/portfolioSettingsView.vue';
import PortfolioAccountsView from '@/view/portfolio/portfolioAccountsView.vue';
import PortfolioExchangeView from '@/view/portfolio/portfolioExchangeView.vue';

@Options({
	name: 'PortfolioView',
	components: {
		PortfolioSettingsView,
		PortfolioAccountsView,
		PortfolioExchangeView,
		PortfolioCurrencyView,
	},
})
export default class PortfolioView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	get balances() {
		return this.virtualState.balances;
	}
}
</script>

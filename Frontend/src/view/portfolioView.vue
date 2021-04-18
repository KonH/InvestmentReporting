<template>
	<portfolio-summary-view />
	<portfolio-currency-view v-for="balance in balances" :key="balance.currency" :currency-code="balance.currency" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioSummaryView from '@/view/portfolioSummaryView.vue';
import PortfolioCurrencyView from '@/view/portfolioCurrencyView.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';

@Options({
	name: 'PortfolioView',
	components: {
		PortfolioSummaryView,
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

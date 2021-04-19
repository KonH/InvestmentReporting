<template>
	<div class="form-group">
		<label>
			Period:
			<select ref="period" v-model="selectedPeriod" class="form-control">
				<option selected>AllTime</option>
				<option>CalendarYear</option>
				<option>RollingYear</option>
				<option>CalendarMonth</option>
				<option>RollingMonth</option>
				<option>CalendarWeek</option>
				<option>RollingWeek</option>
			</select>
		</label>
	</div>
	<portfolio-summary-view />
	<portfolio-currency-view v-for="balance in balances" :key="balance.currency" :currency-code="balance.currency" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioSummaryView from '@/view/portfolioSummaryView.vue';
import PortfolioCurrencyView from '@/view/portfolioCurrencyView.vue';
import { Action, State } from 'vuex-class';
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

	@State('selectedVirtualStatePeriod')
	selectedVirtualStatePeriod!: string;

	@Action('changeSelectedVirtualStatePeriod')
	changeSelectedVirtualStatePeriod!: (period: string) => void;

	selectedPeriod = '';

	mounted() {
		this.selectedPeriod = this.selectedVirtualStatePeriod;
		this.$watch('selectedPeriod', this.changeSelectedVirtualStatePeriod);
	}

	get balances() {
		return this.virtualState.balances;
	}
}
</script>

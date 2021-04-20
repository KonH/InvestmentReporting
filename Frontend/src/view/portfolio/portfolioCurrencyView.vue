<template>
	<h3>{{ currencyCode }}</h3>
	<h4><money :value="virtualSum" :currency-code="currencyCode" /></h4>
	<h5><money-diff :old="realSum" :new="virtualSum" :currency-code="currencyCode" /></h5>
	<portfolio-table :currency-code="currencyCode" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioTable from '@/view/portfolio/portfolioTable.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/common/money.vue';
import MoneyDiff from '@/component/common/moneyDiff.vue';

@Options({
	name: 'PortfolioCurrencyView',
	components: {
		Money,
		MoneyDiff,
		PortfolioTable,
	},
})
export default class PortfolioView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	@Prop()
	currencyCode!: string;

	get balance() {
		const balances = this.virtualState.balances;
		if (balances) {
			const balance = balances.find((b) => b.currency == this.currencyCode);
			if (balance) {
				return balance;
			}
		}
		return null;
	}

	get realSum() {
		return this.balance?.realSum;
	}

	get virtualSum() {
		return this.balance?.virtualSum;
	}
}
</script>

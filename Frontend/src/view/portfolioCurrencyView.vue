<template>
	<h3>{{ currencyCode }}</h3>
	<h4><money :value="virtualSum" :currency-id="currencyId" /></h4>
	<h5><money-diff :old="realSum" :new="virtualSum" :currency-id="currencyId" /></h5>
	<portfolio-table :currency-id="currencyId" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioTable from '@/view/portfolioTable.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';
import { StateDto } from '@/api/state';

@Options({
	name: 'PortfolioCurrencyView',
	components: {
		Money,
		MoneyDiff,
		PortfolioTable,
	},
})
export default class PortfolioView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	@State('virtualState')
	virtualState!: VirtualStateDto;

	@Prop()
	currencyId!: string;

	get currencyCode() {
		return this.activeState.currencies?.find((c) => c.id == this.currencyId)?.code;
	}

	get balance() {
		const balances = this.virtualState.balances;
		if (balances) {
			const balance = balances.find((b) => b.currency == this.currencyId);
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

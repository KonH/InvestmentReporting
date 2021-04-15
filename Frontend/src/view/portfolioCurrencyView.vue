<template>
	<h4><money :value="virtualSum" :currency-id="currencyId" /></h4>
	<h5><money-diff :old="realSum" :new="virtualSum" :currency-id="currencyId" /></h5>
	<table class="table table-sm table-striped">
		<thead>
			<tr>
				<th scope="col">ISIN</th>
				<th scope="col">Name</th>
				<th scope="col">Price</th>
				<th scope="col">Price Change</th>
				<th scope="col">% Price Change</th>
				<th scope="col">Count</th>
				<th scope="col">Sum</th>
				<th scope="col">Sum Change</th>
				<th scope="col">Div/year Sum</th>
				<th scope="col">Div/total Sum</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="asset in inventory" :key="asset.id">
				<portfolio-asset :asset="asset" />
			</tr>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioAsset from '@/component/portfolioAsset.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';
import Money from '@/component/money.vue';
import MoneyDiff from '@/component/moneyDiff.vue';

@Options({
	name: 'PortfolioCurrencyView',
	components: {
		Money,
		MoneyDiff,
		PortfolioAsset,
	},
})
export default class PortfolioView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	@Prop()
	currencyId!: string;

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

	get inventory() {
		const balance = this.balance;
		if (balance) {
			return balance.inventory;
		}
		return [];
	}
}
</script>

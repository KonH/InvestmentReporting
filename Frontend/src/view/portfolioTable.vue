<template>
	<table class="table table-sm table-striped">
		<thead>
			<tr>
				<th scope="col">ISIN</th>
				<th scope="col">Name</th>
				<th class="noselect" scope="col" @click="click('virtualPrice')">{{ orderTag('virtualPrice') }} Price</th>
				<th class="noselect" scope="col" @click="click('priceChange')">{{ orderTag('priceChange') }} Price Change</th>
				<th class="noselect" scope="col" @click="click('priceChangePercent')">{{ orderTag('priceChangePercent') }} % Price Change</th>
				<th class="noselect" scope="col" @click="click('count')">{{ orderTag('count') }} Count</th>
				<th class="noselect" scope="col" @click="click('virtualSum')">{{ orderTag('virtualSum') }} Sum</th>
				<th class="noselect" scope="col" @click="click('sumChange')">{{ orderTag('sumChange') }} Sum Change</th>
				<th class="noselect" scope="col" @click="click('yearDividend')">{{ orderTag('yearDividend') }} Div/year Sum</th>
				<th class="noselect" scope="col" @click="click('dividendSum')">{{ orderTag('dividendSum') }} Div/total Sum</th>
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
import { VirtualAssetDto, VirtualStateDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';

interface Indexed {
	[key: string]: number | undefined;
}

@Options({
	name: 'PortfolioTable',
	components: {
		PortfolioAsset,
	},
})
export default class PortfolioView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	@Prop()
	currencyCode!: string;

	orderTarget = '';
	orderDescending = false;

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

	get inventory() {
		const balance = this.balance;
		if (balance && balance.inventory) {
			return this.order(balance.inventory);
		}
		return [];
	}

	order(inventory: VirtualAssetDto[]) {
		if (this.orderTarget) {
			const array = Array.from(inventory);
			return array.sort(this.compare);
		}
		return inventory;
	}

	compare(left: VirtualAssetDto, right: VirtualAssetDto) {
		const targetValueLeft = this.getValue(left);
		const targetValueRight = this.getValue(right);
		const result = targetValueLeft - targetValueRight;
		return this.orderDescending ? -result : result;
	}

	getValue(asset: VirtualAssetDto) {
		const indexed = asset as Indexed;
		const valueAtTarget = indexed[this.orderTarget];
		if (!valueAtTarget) {
			return this.getCalculatedValue(asset);
		}
		return valueAtTarget;
	}

	getCalculatedValue(asset: VirtualAssetDto) {
		switch (this.orderTarget) {
			case 'priceChange':
			case 'priceChangePercent':
			case 'sumChange':
				return (asset.virtualPrice ?? 0) - (asset.realPrice ?? 0);
		}
		return 0;
	}

	click(target: string) {
		if (target == this.orderTarget) {
			this.reverseOrDisableOrder();
			return;
		}
		this.orderTarget = target;
		this.applyOrder();
	}

	reverseOrDisableOrder() {
		if (this.orderDescending) {
			this.orderDescending = false;
		} else {
			this.orderTarget = '';
		}
	}

	applyOrder() {
		this.orderDescending = true;
	}

	orderTag(target: string) {
		if (target == this.orderTarget) {
			return this.orderDescending ? '▼' : '▲';
		}
		return '';
	}
}
</script>
<style>
.noselect {
	-webkit-touch-callout: none; /* iOS Safari */
	-webkit-user-select: none; /* Safari */
	-moz-user-select: none; /* Firefox */
	-ms-user-select: none; /* Internet Explorer/Edge */
	user-select: none; /* Non-prefixed version, currently supported by Chrome and Opera */
}
</style>

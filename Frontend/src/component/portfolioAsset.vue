<template>
	<!-- ISIN -->
	<th scope="row">{{ asset.isin }}</th>
	<!-- Name -->
	<td>{{ asset.name }}</td>
	<!-- Price -->
	<td>
		<money :value="asset.virtualPrice" :currency-id="asset.currency" />
	</td>
	<!-- Price Change -->
	<td>
		<money-value-diff :old="asset.realPrice" :new="asset.virtualPrice" :currency-id="asset.currency" />
	</td>
	<!-- % Price Change -->
	<td>
		<percent-diff :old="asset.realPrice" :new="asset.virtualPrice" />
	</td>
	<!-- Count -->
	<td>{{ asset.count }}</td>
	<!-- Sum -->
	<td>
		<money :value="asset.virtualSum" :currency-id="asset.currency" />
	</td>
	<!-- Sum Change -->
	<td>
		<money-value-diff :old="asset.realSum" :new="asset.virtualSum" :currency-id="asset.currency" />
	</td>
	<!-- Div/year Sum -->
	<td>N/A</td>
	<!-- Div/total Sum -->
	<td>N/A</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { VirtualAssetDto } from '@/api/market';
import Money from '@/component/money.vue';
import MoneyValueDiff from '@/component/moneyValueDiff.vue';
import PercentDiff from '@/component/percentDiff.vue';

@Options({
	name: 'PortfolioAsset',
	components: {
		MoneyValueDiff,
		Money,
		PercentDiff,
	},
})
export default class PortfolioAsset extends Vue {
	@Prop()
	asset!: VirtualAssetDto;
}
</script>

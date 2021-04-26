<template>
	<!-- ISIN -->
	<th scope="row">{{ asset.isin }}</th>
	<!-- Name -->
	<td>{{ asset.name }}</td>
	<!-- Price -->
	<td>
		<money :value="asset.virtualPrice" :currency-code="asset.currency" />
	</td>
	<!-- Price Change -->
	<td>
		<money-value-diff :old="asset.realPrice" :new="asset.virtualPrice" :currency-code="asset.currency" />
	</td>
	<!-- % Price Change -->
	<td>
		<percent-diff :old="asset.realPrice" :new="asset.virtualPrice" />
	</td>
	<!-- Count -->
	<td>{{ asset.count }}</td>
	<!-- Sum -->
	<td>
		<money :value="asset.virtualSum" :currency-code="asset.currency" />
	</td>
	<!-- Sum Change -->
	<td>
		<money-value-diff :old="asset.realSum" :new="asset.virtualSum" :currency-code="asset.currency" />
	</td>
	<!-- Div/last Sum -->
	<td>
		<money :value="asset.dividend.lastDividend" :currency-code="asset.currency" />
	</td>
	<!-- Div/last Change -->
	<td>
		<money-diff :old="asset.dividend.previousDividend" :new="asset.dividend.lastDividend" :currency-code="asset.currency" />
	</td>
	<!-- Div/year Sum -->
	<td>
		<money :value="asset.dividend.yearDividend" :currency-code="asset.currency" />
	</td>
	<!-- Div/total Sum -->
	<td>
		<money :value="asset.dividend.dividendSum" :currency-code="asset.currency" />
	</td>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import { VirtualAssetDto } from '@/api/market';
import Money from '@/component/common/money.vue';
import MoneyValueDiff from '@/component/common/moneyValueDiff.vue';
import PercentDiff from '@/component/common/percentDiff.vue';
import MoneyDiff from '@/component/common/moneyDiff.vue';

@Options({
	name: 'PortfolioAsset',
	components: {
		MoneyValueDiff,
		Money,
		MoneyDiff,
		PercentDiff,
	},
})
export default class PortfolioAsset extends Vue {
	@Prop()
	asset!: VirtualAssetDto;
}
</script>

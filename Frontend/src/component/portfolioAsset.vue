<template>
	<!-- ISIN -->
	<th scope="row">{{ asset.isin }}</th>
	<!-- Name -->
	<td>{{ asset.name }}</td>
	<!-- Price -->
	<td>N/A</td>
	<!-- Price Change -->
	<td>N/A</td>
	<!-- % Price Change -->
	<td>N/A</td>
	<!-- Count -->
	<td>{{ asset.count }}</td>
	<!-- Sum -->
	<td>{{ asset.virtualPrice }}</td>
	<!-- Sum Change -->
	<td>
		<money-value-diff :old="asset.realPrice" :new="asset.virtualPrice" :currency-id="asset.currency" />
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
import MoneyValueDiff from '@/component/moneyValueDiff.vue';

@Options({
	name: 'PortfolioAsset',
	components: {
		MoneyValueDiff,
	},
})
export default class PortfolioAsset extends Vue {
	@Prop()
	asset: VirtualAssetDto;
}
</script>

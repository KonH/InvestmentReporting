<template>
	<b>{{ asset.isin }} {{ asset.type }} {{ asset.name }} x{{ asset.count }} ({{ asset.realPrice }} => {{ asset.virtualPrice }})</b>
	<router-link :to="`/broker/${asset.broker}/asset/${asset.id}/operations`" class="btn btn-secondary ml-2"
		>Operations</router-link
	>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { VirtualAssetDto } from '@/api/market';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'VirtualAsset',
})
export default class VirtualAsset extends Vue {
	@Prop()
	asset!: VirtualAssetDto;
}
</script>

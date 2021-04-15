<template>
	<b>{{ asset.isin }} {{ asset.name }} x{{ asset.count }}</b>
	<router-link :to="`/broker/${brokerId}/asset/${asset.id}/operations`" class="btn btn-secondary ml-2">Operations</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { AssetDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'Asset',
})
export default class Asset extends Vue {
	@Prop()
	asset!: AssetDto;

	@Prop()
	brokerId!: string;
}
</script>
